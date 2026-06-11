from fastapi import FastAPI, File, UploadFile

from app.adapters.ml.tesseract_comparendo import extract_comparendo

app = FastAPI(title="FLIT python-ml", version="1.0.0")


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}


@app.post("/ocr/comparendo:extract")
async def ocr_comparendo_extract(file: UploadFile = File(...)) -> dict:
    content = await file.read()
    content_type = file.content_type or "application/octet-stream"
    return extract_comparendo(content, content_type, file.filename or "upload")
