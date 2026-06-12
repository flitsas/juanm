"""OCR de comparendos con Tesseract 5 + PyMuPDF (texto embebido) + pdf2image (escaneados)."""

from __future__ import annotations

import io
import re
from dataclasses import dataclass

from PIL import Image, UnidentifiedImageError

from app.adapters.ml.ocr_runtime import configure_ocr_runtime


@dataclass(frozen=True)
class ComparendoOcrResult:
    numero_comparendo: str | None
    placa: str | None
    documento: str | None
    infractor: str | None
    fecha: str | None
    valor: float | None
    infraccion: str | None
    confidence: float
    raw_text: str

    def to_dict(self) -> dict:
        return {
            "numeroComparendo": self.numero_comparendo,
            "placa": self.placa,
            "documento": self.documento,
            "infractor": self.infractor,
            "fecha": self.fecha,
            "valor": self.valor,
            "infraccion": self.infraccion,
            "confidence": self.confidence,
            "rawText": self.raw_text,
        }


def _is_pdf(content: bytes, content_type: str) -> bool:
    return content_type == "application/pdf" or content.startswith(b"%PDF")


def _extract_pdf_text_layer(content: bytes) -> str | None:
    try:
        import fitz

        doc = fitz.open(stream=content, filetype="pdf")
        try:
            if doc.page_count == 0:
                return None
            text = doc[0].get_text("text").strip()
            return text or None
        finally:
            doc.close()
    except Exception:
        return None


def _pdf_to_image(content: bytes) -> tuple[Image.Image | None, str | None]:
    try:
        from pdf2image import convert_from_bytes

        _, poppler_path = configure_ocr_runtime()
        if poppler_path:
            pages = convert_from_bytes(
                content, first_page=1, last_page=1, dpi=300, poppler_path=poppler_path
            )
        else:
            pages = convert_from_bytes(content, first_page=1, last_page=1, dpi=300)
        if pages:
            return pages[0], None
        return None, "pdf2image no produjo páginas."
    except Exception as exc:
        message = str(exc).lower()
        if "poppler" in message or "pdftoppm" in message or "not installed" in message:
            return None, (
                "Poppler no está instalado o no está en PATH. "
                "En Windows: winget install oschwartz10612.Poppler"
            )
        return None, f"No se pudo convertir PDF a imagen: {exc}"


def _load_image_bytes(content: bytes, content_type: str) -> tuple[Image.Image | None, str | None]:
    if _is_pdf(content, content_type):
        image, error = _pdf_to_image(content)
        if image is not None:
            return image, None
        return None, error or "PDF escaneado sin conversión a imagen."

    try:
        return Image.open(io.BytesIO(content)), None
    except UnidentifiedImageError:
        return None, "Formato de imagen no reconocido."


def _extract_text(image: Image.Image) -> tuple[str, float, str | None]:
    try:
        import pytesseract
    except Exception:
        return "", 0.0, "pytesseract no está disponible."

    tesseract_cmd, _ = configure_ocr_runtime()
    if not tesseract_cmd:
        return (
            "",
            0.0,
            "Tesseract OCR no está instalado. Windows: winget install UB-Mannheim.TesseractOCR",
        )

    try:
        pytesseract.pytesseract.tesseract_cmd = tesseract_cmd
        pytesseract.get_tesseract_version()
    except Exception as exc:
        return "", 0.0, f"Tesseract no responde ({tesseract_cmd}): {exc}"

    langs = ("spa+eng", "eng")
    last_error: Exception | None = None
    for lang in langs:
        try:
            text = pytesseract.image_to_string(image, lang=lang)
            cleaned = text.strip()
            if cleaned:
                return cleaned, 0.75, None
        except Exception as exc:
            last_error = exc

    if last_error:
        return "", 0.0, f"Error ejecutando Tesseract: {last_error}"
    return "", 0.0, "Tesseract no detectó texto en la imagen."


def _parse_numero_comparendo(raw_text: str) -> str | None:
    patterns = [
        r"(?:comparendo|citaci[oó]n)\s*(?:no\.?|n[uú]mero)?[:\s#]*([A-Z]?\d{14,22})",
        r"NOTIFICACI[OÓ]N.*?COMPARENDO\s+No\.\s*(\d{14,22})",
        r"ORDEN\s+DE\s+COMPARENDO\s+(\d{14,22})",
        r"\bNo\s+(D\d{18,22})\b",
        r"\b(\d{17})\b",
        r"\b(\d{14,16})\b",
    ]
    for pattern in patterns:
        match = _first_match(raw_text, pattern, re.I | re.S)
        if match and not match.lower().startswith("no"):
            return match.upper() if match[0].isalpha() else match
    return None


def _parse_placa(raw_text: str) -> str | None:
    patterns = [
        r"Placa[:\s]+([A-Z]{2,3}\s?\d{3})",
        r"PLACA\s*\|[^|\n]*\|?\s*([A-Z]{2,3}\d{3})",
        r"Placa:\s*([A-Z]{3}\d{3})",
    ]
    for pattern in patterns:
        match = _first_match(raw_text, pattern, re.I)
        if match:
            return match.replace(" ", "").upper()

    for candidate in re.findall(r"\b([A-Z]{3}\d{3})\b", raw_text):
        if candidate not in {"KM500", "AND500"}:
            return candidate
    return None


def _parse_documento(raw_text: str) -> str | None:
    patterns = [
        r"NIT\s*\|?\s*(\d{8,11})",
        r"(?:c[eé]dula|documento|cc)\s*[:\s|]*(\d{6,12})",
        r"Tipo y No\.\s*Identificaci[oó]n\s*\n[^\n]+\s+NIT\s+(\d{8,11})",
    ]
    for pattern in patterns:
        match = _first_match(raw_text, pattern, re.I)
        if match:
            return match
    return None


def _parse_infractor(raw_text: str) -> str | None:
    patterns = [
        r"Respetado\(a\)\s+señor\(a\)\s+([A-Z][A-Za-zÁÉÍÓÚáéíóúñÑ\s.]+?)(?:\s+www|\s+La\s+)",
        r"Nombre\s+Tipo y No\.\s*Identificaci[oó]n\s*\n([A-Z][A-Za-zÁÉÍÓÚáéíóúñÑ\s.]+?)\s+NIT",
        r"\|\s*([A-Z][A-Za-zÁÉÍÓÚáéíóúñÑ\s.]{4,40})\s*\|\s*\n\s*\|\s*NIT",
        r"(BANCOLOMBIA\s*S\.?\s*A\.?)",
    ]
    for pattern in patterns:
        match = _first_match(raw_text, pattern, re.I | re.S)
        if match and match.lower() not in {"completo", "tipo y no"}:
            return re.sub(r"\s+", " ", match).strip()
    return None


def _parse_fecha(raw_text: str) -> str | None:
    patterns = [
        r"Fecha de (?:imposici[oó]n|comparendo|infracci[oó]n)\s+(\d{1,2}\s+de\s+\w+\s+de\s+\d{4})",
        r"Fecha Hora Infracci[oó]n\s*\n(\d{2}/\d{2}/\d{4})",
        r"\b(\d{2}/\d{2}/\d{4})\b",
        r"\b(\d{4}[-/]\d{2}[-/]\d{2})\b",
    ]
    for pattern in patterns:
        match = _first_match(raw_text, pattern, re.I)
        if match:
            return match.strip()
    return None


def _parse_valor(raw_text: str) -> float | None:
    patterns = [
        r"PAGO CON DESCUENTO DEL 50%[^\$]*\$\s*([\d.,]+)",
        r"(?:valor|total)[:\s]*\$?\s*([\d.,]+)",
        r"\$\s*([\d]{1,3}(?:\.[\d]{3})+)",
    ]
    for pattern in patterns:
        match = _first_match(raw_text, pattern, re.I | re.S)
        if match:
            normalized = match.replace(".", "").replace(",", ".")
            try:
                return float(normalized)
            except ValueError:
                continue
    return None


def _parse_infraccion(raw_text: str) -> str | None:
    patterns = [
        r"C[oó]digo Infracci[oó]n\s*\n?\s*([CD]\d{2})",
        r"C[oó]DIGO DE INFRACCI[OÓ]N[^\n]*\n[^\n]*\|[^\n]*\|\s*([CD]\d{2})",
        r"\b([CD]\d{2})[-\s]",
    ]
    for pattern in patterns:
        match = _first_match(raw_text, pattern, re.I)
        if match:
            return match.upper()
    return None


def _parse_fields(raw_text: str) -> ComparendoOcrResult:
    return ComparendoOcrResult(
        numero_comparendo=_parse_numero_comparendo(raw_text),
        placa=_parse_placa(raw_text),
        documento=_parse_documento(raw_text),
        infractor=_parse_infractor(raw_text),
        fecha=_parse_fecha(raw_text),
        valor=_parse_valor(raw_text),
        infraccion=_parse_infraccion(raw_text),
        confidence=0.75 if raw_text else 0.0,
        raw_text=raw_text,
    )


def _first_match(text: str, pattern: str, flags: int = 0) -> str | None:
    match = re.search(pattern, text, flags)
    return match.group(1).strip() if match else None


def _build_unavailable_result(filename: str, reason: str) -> dict:
    return ComparendoOcrResult(
        numero_comparendo=None,
        placa=None,
        documento=None,
        infractor=None,
        fecha=None,
        valor=None,
        infraccion=None,
        confidence=0.0,
        raw_text=f"OCR no disponible para {filename}: {reason}",
    ).to_dict()


def extract_comparendo(content: bytes, content_type: str, filename: str) -> dict:
    warnings: list[str] = []

    if _is_pdf(content, content_type):
        embedded = _extract_pdf_text_layer(content)
        if embedded:
            parsed = _parse_fields(embedded)
            return ComparendoOcrResult(
                numero_comparendo=parsed.numero_comparendo,
                placa=parsed.placa,
                documento=parsed.documento,
                infractor=parsed.infractor,
                fecha=parsed.fecha,
                valor=parsed.valor,
                infraccion=parsed.infraccion,
                confidence=0.85,
                raw_text=embedded,
            ).to_dict()
        warnings.append("PDF sin capa de texto; se requiere OCR por imagen.")

    image, load_error = _load_image_bytes(content, content_type)
    if image is None:
        return _build_unavailable_result(filename, load_error or "no se pudo cargar el archivo.")

    raw_text, confidence, ocr_error = _extract_text(image)
    if ocr_error:
        warnings.append(ocr_error)

    if not raw_text:
        reason = " ".join(warnings) if warnings else "no se detectó texto en el documento."
        return _build_unavailable_result(filename, reason)

    parsed = _parse_fields(raw_text)
    return ComparendoOcrResult(
        numero_comparendo=parsed.numero_comparendo,
        placa=parsed.placa,
        documento=parsed.documento,
        infractor=parsed.infractor,
        fecha=parsed.fecha,
        valor=parsed.valor,
        infraccion=parsed.infraccion,
        confidence=confidence,
        raw_text=raw_text,
    ).to_dict()
