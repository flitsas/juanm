"""
Genera plantilla AcroForm vialix-dp-template.pdf — Feature #9564 GDC-PLANTILLAS.
Diseño alineado a tokens FLIT (globals.css / flitready-suite).
Regenerar: python generate_vialix_dp_template.py
"""

from __future__ import annotations

from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.lib.units import mm
from reportlab.pdfgen import canvas

# Tokens FLIT
FLIT_ACTION = colors.HexColor("#003EFF")
FLIT_LIME = colors.HexColor("#B3FF1F")
FLIT_DEEP = colors.HexColor("#162744")
FLIT_SECONDARY = colors.HexColor("#59677D")
FLIT_BORDER = colors.HexColor("#D9DEE8")
FLIT_CARD = colors.HexColor("#FFFFFF")
FLIT_TABLE_HEAD = colors.HexColor("#D0DCEF")
FLIT_TECH = colors.HexColor("#00DBD5")

OUTPUT = Path(__file__).resolve().parent / "vialix-dp-template.pdf"

# Campos AcroForm — nombres alineados al catálogo variables sistema (design.md D5)
TEXT_FIELDS = [
    ("tenant_nombre", "Compañía / Tenant"),
    ("comparendo_numero", "Número de comparendo"),
    ("comparendo_placa", "Placa"),
    ("comparendo_documento", "Documento infractor"),
    ("comparendo_infractor_nombre", "Nombre infractor (comparendo)"),
    ("comparendo_fecha_comparendo", "Fecha comparendo"),
    ("comparendo_fecha_notificacion", "Fecha notificación"),
    ("comparendo_total", "Valor total comparendo"),
    ("contraventor_nombre", "Nombre contraventor"),
    ("contraventor_documento", "Documento contraventor"),
    ("contraventor_correo", "Correo contraventor"),
    ("secretaria_destino", "Secretaría de tránsito destino"),
]

CHOICE_FIELD = (
    "comparendo_estado",
    "Estado del comparendo",
    ["Pendiente", "Notificado", "En trámite", "Pagado", "Cerrado"],
)


def draw_header(c: canvas.Canvas, page_w: float, page_h: float) -> None:
    c.setFillColor(FLIT_LIME)
    c.rect(0, page_h - 6 * mm, page_w, 6 * mm, fill=1, stroke=0)

    c.setFillColor(FLIT_ACTION)
    c.rect(0, page_h - 34 * mm, page_w, 28 * mm, fill=1, stroke=0)

    c.setFillColor(colors.white)
    c.setFont("Helvetica-Bold", 14)
    c.drawString(18 * mm, page_h - 22 * mm, "FLIT vialix")
    c.setFont("Helvetica", 9)
    c.drawString(18 * mm, page_h - 28 * mm, "Generador documental jurídico · GDC-PLANTILLAS")

    c.setFillColor(FLIT_TECH)
    c.circle(page_w - 22 * mm, page_h - 22 * mm, 8 * mm, fill=1, stroke=0)


def draw_card_frame(c: canvas.Canvas, page_w: float, page_h: float) -> None:
    margin = 14 * mm
    top = page_h - 42 * mm
    bottom = 32 * mm
    c.setFillColor(FLIT_CARD)
    c.setStrokeColor(FLIT_BORDER)
    c.setLineWidth(0.8)
    c.roundRect(margin, bottom, page_w - 2 * margin, top - bottom, 5 * mm, fill=1, stroke=1)


def draw_title_block(c: canvas.Canvas, page_w: float, page_h: float) -> float:
    y = page_h - 52 * mm
    c.setFillColor(FLIT_DEEP)
    c.setFont("Helvetica-Bold", 18)
    c.drawString(22 * mm, y, "DERECHO DE PETICIÓN")
    c.setFont("Helvetica", 10)
    c.setFillColor(FLIT_SECONDARY)
    c.drawString(
        22 * mm,
        y - 7 * mm,
        "Correspondencia legal · Comparendo y contraventor · Plantilla parametrizable",
    )
    c.setStrokeColor(FLIT_TABLE_HEAD)
    c.setLineWidth(2)
    c.line(22 * mm, y - 11 * mm, page_w - 22 * mm, y - 11 * mm)
    return y - 18 * mm


def add_text_field(
    form,
    c: canvas.Canvas,
    name: str,
    label: str,
    x: float,
    y: float,
    width: float,
) -> None:
    c.setFillColor(FLIT_SECONDARY)
    c.setFont("Helvetica", 8)
    c.drawString(x, y + 9 * mm, label)

    form.textfield(
        name=name,
        tooltip=name,
        x=x,
        y=y,
        width=width,
        height=7 * mm,
        borderStyle="underlined",
        borderColor=FLIT_BORDER,
        fillColor=colors.white,
        textColor=FLIT_DEEP,
        forceBorder=True,
        fontSize=10,
        fieldFlags="doNotScroll",
    )


def draw_footer(c: canvas.Canvas, page_w: float) -> None:
    c.setFillColor(FLIT_TABLE_HEAD)
    c.rect(0, 0, page_w, 22 * mm, fill=1, stroke=0)
    c.setFillColor(FLIT_SECONDARY)
    c.setFont("Helvetica", 7)
    c.drawCentredString(
        page_w / 2,
        10 * mm,
        "Documento generado por FLIT vialix · Uso exclusivo trámite ante secretaría de tránsito",
    )
    c.setFillColor(FLIT_ACTION)
    c.setFont("Helvetica-Bold", 7)
    c.drawRightString(page_w - 14 * mm, 5 * mm, "gdc.flit.co")


def main() -> None:
    page_w, page_h = A4
    c = canvas.Canvas(str(OUTPUT), pagesize=A4)
    form = c.acroForm

    draw_header(c, page_w, page_h)
    draw_card_frame(c, page_w, page_h)
    y_start = draw_title_block(c, page_w, page_h)

    col_w = (page_w - 50 * mm) / 2
    left_x = 22 * mm
    right_x = 22 * mm + col_w + 6 * mm
    row_h = 16 * mm

    for index, (field_name, label) in enumerate(TEXT_FIELDS):
        col = index % 2
        row = index // 2
        x = left_x if col == 0 else right_x
        y = y_start - row * row_h - 7 * mm
        add_text_field(form, c, field_name, label, x, y, col_w - 4 * mm)

    choice_name, choice_label, options = CHOICE_FIELD
    choice_y = y_start - ((len(TEXT_FIELDS) + 1) // 2) * row_h - 4 * mm
    c.setFillColor(FLIT_SECONDARY)
    c.setFont("Helvetica", 8)
    c.drawString(left_x, choice_y + 9 * mm, choice_label)
    form.choice(
        name=choice_name,
        tooltip=choice_name,
        value=options[0],
        options=options,
        x=left_x,
        y=choice_y,
        width=col_w - 4 * mm,
        height=7 * mm,
        borderStyle="underlined",
        borderColor=FLIT_BORDER,
        fillColor=colors.white,
        textColor=FLIT_DEEP,
        forceBorder=True,
        fontSize=10,
    )

    body_y = choice_y - 22 * mm
    c.setFillColor(FLIT_DEEP)
    c.setFont("Helvetica-Bold", 9)
    c.drawString(22 * mm, body_y, "Solicitud")
    c.setFont("Helvetica", 9)
    c.setFillColor(FLIT_SECONDARY)
    text = (
        "Por medio del presente documento, la compañía identificada solicita información y "
        "actuación respecto del comparendo indicado, conforme a la normativa de tránsito "
        "y los procedimientos de la secretaría destino."
    )
    c.drawString(22 * mm, body_y - 6 * mm, text[:95])
    c.drawString(22 * mm, body_y - 12 * mm, text[95:])

    draw_footer(c, page_w)
    c.showPage()
    c.save()
    print(f"Generated {OUTPUT} ({OUTPUT.stat().st_size} bytes)")


if __name__ == "__main__":
    main()
