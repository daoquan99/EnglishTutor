# System API

## GET /health

- Endpoint: `GET /health`
- Purpose: Report whether the API host is running and able to answer basic health checks.
- Auth requirement: Anonymous.
- Request body: None.
- Response body: Plain text health check response from ASP.NET Core health checks, typically `Healthy`.
- Validation rules: None.
- Error codes:
  - `503 Service Unavailable` if a registered health check reports unhealthy.
- Application flow:
  - Request reaches `EnglishTutor.Api`.
  - ASP.NET Core health checks execute.
  - The endpoint returns the aggregate health status.
- Related modules: None. This is host-level infrastructure.
- Integration events produced: None.
- Integration events consumed: None.
- Read models/projections updated: None.

