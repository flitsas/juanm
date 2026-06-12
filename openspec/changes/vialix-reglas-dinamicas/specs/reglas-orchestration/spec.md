# reglas-orchestration

## ADDED Requirements

### Requirement: PDF generation

For each match, the system SHALL render a PDF from the linked GDC template substituting comparendo field tags.

### Requirement: Individual email with attachment

The system SHALL send one email per comparendo to the secretariat contact resolved for that comparendo, with the PDF attached, via NOTIF integration.

### Requirement: Processing traceability

The system SHALL persist per rule-comparendo processing records with timestamp, status, document reference, and email reference.

### Requirement: Execution logs and metrics

Each run SHALL log start/end, errors, and counters for evaluated comparendos, matches, PDFs generated, and emails sent.
