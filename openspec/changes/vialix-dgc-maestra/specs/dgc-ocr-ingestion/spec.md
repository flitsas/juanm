## ADDED Requirements

### Requirement: OCR file upload

The system SHALL accept individual or batch upload of comparendo files in PDF, PNG, and JPG formats.

#### Scenario: Successful upload to buffer

- **WHEN** valid PDF, PNG, or JPG files are uploaded
- **THEN** the API creates OCR buffer items with OCR-extracted fields editable before persistence

### Requirement: Pre-save validation buffer

The system SHALL expose a validation buffer allowing operators to review and correct OCR fields before confirming save.

#### Scenario: Confirm after edit

- **WHEN** the operator confirms save from the buffer with valid required fields
- **THEN** a comparendo record is persisted

### Requirement: Duplicate rejection on confirm

The system SHALL return a duplicate error when confirming save if the comparendo number already exists in the tenant.

#### Scenario: Duplicate on confirm

- **WHEN** the operator confirms a buffer item whose comparendo number already exists
- **THEN** the API returns a duplicate error and does not insert the record
