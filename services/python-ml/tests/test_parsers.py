"""Tests de las funciones puras de parsing de comparendos (sin Tesseract/Poppler)."""

from __future__ import annotations

from app.adapters.ml import tesseract_comparendo as tc


def test_parse_numero_comparendo_con_etiqueta() -> None:
    assert tc._parse_numero_comparendo("Comparendo No. 11001000000012345678") == (
        "11001000000012345678"
    )


def test_parse_numero_comparendo_con_prefijo_alfabetico() -> None:
    assert tc._parse_numero_comparendo("No D1234567890123456789") == "D1234567890123456789"


def test_parse_numero_comparendo_sin_match() -> None:
    assert tc._parse_numero_comparendo("documento sin numero") is None


def test_parse_placa_variantes() -> None:
    assert tc._parse_placa("Placa: ABC123") == "ABC123"
    assert tc._parse_placa("texto ABC123 suelto") == "ABC123"


def test_parse_placa_excluye_falsos_positivos() -> None:
    assert tc._parse_placa("distancia KM500 marcada") is None


def test_parse_documento() -> None:
    assert tc._parse_documento("Cedula: 1098765432") == "1098765432"
    assert tc._parse_documento("NIT 900123456") == "900123456"


def test_parse_infractor() -> None:
    texto = "Respetado(a) señor(a) JUAN PEREZ www.algo.com"
    assert tc._parse_infractor(texto) == "JUAN PEREZ"


def test_parse_fecha() -> None:
    assert tc._parse_fecha("Fecha 12/05/2024 registrada") == "12/05/2024"
    assert tc._parse_fecha("emitido 2024-05-12 hoy") == "2024-05-12"


def test_parse_valor() -> None:
    assert tc._parse_valor("Valor: $ 390.000") == 390000.0


def test_parse_valor_sin_match() -> None:
    assert tc._parse_valor("sin importe") is None


def test_parse_infraccion() -> None:
    assert tc._parse_infraccion("C29 - conduccion") == "C29"


def test_first_match_y_helpers() -> None:
    assert tc._first_match("abc 123 def", r"(\d+)") == "123"
    assert tc._first_match("nada", r"(\d+)") is None


def test_is_pdf() -> None:
    assert tc._is_pdf(b"%PDF-1.7", "application/octet-stream") is True
    assert tc._is_pdf(b"\x89PNG", "application/pdf") is True
    assert tc._is_pdf(b"\x89PNG", "image/png") is False


def test_parse_fields_y_to_dict() -> None:
    result = tc._parse_fields("Placa: ABC123 Valor: $ 100.000")
    payload = result.to_dict()
    assert payload["placa"] == "ABC123"
    assert payload["valor"] == 100000.0
    assert payload["confidence"] == 0.75
    assert "rawText" in payload


def test_parse_fields_texto_vacio_confidence_cero() -> None:
    assert tc._parse_fields("").confidence == 0.0


def test_build_unavailable_result() -> None:
    payload = tc._build_unavailable_result("foo.pdf", "Poppler ausente")
    assert payload["confidence"] == 0.0
    assert "foo.pdf" in payload["rawText"]
    assert payload["numeroComparendo"] is None
