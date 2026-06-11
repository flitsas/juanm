## ADDED Requirements

### Requirement: Master list with fifteen columns

The system SHALL return a paginated master list including Estado, No. Comparendo, Infractor, Nro Documento, Placa, Infracción, Fecha Comparendo, Fecha Notificación, Días Restantes, Secretaría, Total, Pago, Contraventor, DP (reference), and Fuente.

#### Scenario: List comparendos

- **WHEN** an authenticated tenant user requests the DGC master list
- **THEN** the response includes all fifteen operational columns per comparendo

### Requirement: Real-time discount days remaining

The system SHALL calculate Días Restantes from notification date and discount matrix (global or per secretaría).

#### Scenario: Days remaining calculation

- **WHEN** a comparendo detail is requested with a configured discount matrix
- **THEN** Días Restantes reflects the parameterized discount window in real time

### Requirement: Filtered paginated search

The system SHALL support filtering by estado, placa, and date range with pagination and sort order.

#### Scenario: Filter by estado

- **WHEN** the client requests the list filtered by estado
- **THEN** only comparendos matching the estado are returned with pagination metadata
