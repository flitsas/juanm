## ADDED Requirements

### Requirement: Master table UI

The system SHALL render the DGC master table with fifteen columns, global search, estado/fecha/secretaría filters, and pagination per Flit Ready prototype.

#### Scenario: Authenticated DGC view

- **WHEN** an authenticated user navigates to the DGC section
- **THEN** the master table is displayed with search, filters, and pagination

### Requirement: Empty state

The system SHALL display an actionable empty state when no comparendos exist for the tenant.

#### Scenario: No comparendos

- **WHEN** the tenant has no comparendos and the DGC module loads
- **THEN** an empty state with actionable guidance is shown

### Requirement: OCR upload dialog

The system SHALL provide a dialog with dropzone for PDF/PNG/JPG and editable preview per detected comparendo before save.

#### Scenario: Mass upload dialog

- **WHEN** the operator opens the mass upload dialog and selects files
- **THEN** editable previews are shown for each detected comparendo

### Requirement: Detail panel with tabs

The system SHALL show a side panel with Detalle, Contraventor, and Log de correos tabs when a row is selected.

#### Scenario: Open detail sheet

- **WHEN** the operator selects a comparendo row
- **THEN** the detail panel opens with the three tabs

### Requirement: HTML evidence modal

The system SHALL render stored email HTML in a modal when a log entry with evidence is opened.

#### Scenario: Evidence modal

- **WHEN** the operator opens a log entry that has HTML evidence
- **THEN** a modal displays the sanitized full HTML message

### Requirement: DP read-only column

The system SHALL show DP status or read-only link without document generation capability.

#### Scenario: DP reference display

- **WHEN** a comparendo has associated petition references
- **THEN** the DP column or tab shows read-only status or link only
