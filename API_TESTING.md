# API Testing Guide

## 1. Start the API

Set `EmployeeLeaveManagement.API` as the startup project and run it, or use:

```powershell
dotnet run --project src/API
```

Default development addresses:

- Swagger: `https://localhost:7279/swagger`
- HTTP API: `http://localhost:5266`
- HTTPS API: `https://localhost:7279`

If the database has not been seeded, run once:

```powershell
$env:Database__ApplyMigrationsOnStartup = "true"
dotnet run --project src/API
```

## 2. Seed accounts

| Role | Email | Password |
|---|---|---|
| Admin | `admin@leave.local` | `Admin123!` |
| Manager | `manager@leave.local` | `Manager123!` |
| Employee | `employee@leave.local` | `Employee123!` |

## 3. Swagger authentication

1. Call `POST /api/auth/login` with one of the accounts above.
2. Copy only the `accessToken` value from the response.
3. Select **Authorize** in Swagger.
4. Paste the token and select **Authorize**.

## 4. Authentication endpoints

### Register an Employee

`POST /api/auth/register` - anonymous access

Use an existing active department ID from `GET /api/departments`. The seeded
Engineering department ID in the current database is
`d98f9d6d-d617-4b82-82eb-2ff6f3aef6e9`.

```json
{
  "email": "new.employee@leave.local",
  "password": "Employee123!",
  "employeeNumber": "EMP-002",
  "firstName": "New",
  "lastName": "Employee",
  "departmentId": "d98f9d6d-d617-4b82-82eb-2ff6f3aef6e9"
}
```

Expected: `201 Created`. Registration always creates the Employee role.

### Login

`POST /api/auth/login` - anonymous access

```json
{
  "email": "admin@leave.local",
  "password": "Admin123!"
}
```

Expected: `200 OK` with `accessToken` and `expiresAtUtc`.

### Logout

`POST /api/auth/logout` - authenticated access

No body is required. Expected: `204 No Content`. Any later request with the same
token must return `401 Unauthorized`.

## 5. Employee endpoints

### List employees

`GET /api/employees` - Admin or Manager

- Admin sees all employees.
- Manager sees employees in their department only.
- Employee receives `403 Forbidden`.

### Get an employee

`GET /api/employees/{id}` - Admin or Manager

Current seed IDs can be obtained from the list endpoint.

### Create an employee

`POST /api/employees` - Admin only

```json
{
  "email": "second.employee@leave.local",
  "password": "Employee123!",
  "employeeNumber": "EMP-003",
  "firstName": "Second",
  "lastName": "Employee",
  "departmentId": "d98f9d6d-d617-4b82-82eb-2ff6f3aef6e9",
  "managerId": "d91e1506-cb7f-4d5a-9b95-fdbc9fecc3a0",
  "role": "Employee"
}
```

Allowed role values are `Employee` and `Manager`. Expected: `201 Created`.

### Update an employee

`PUT /api/employees/{id}` - Admin only

```json
{
  "firstName": "Updated",
  "lastName": "Employee",
  "departmentId": "d98f9d6d-d617-4b82-82eb-2ff6f3aef6e9",
  "managerId": "d91e1506-cb7f-4d5a-9b95-fdbc9fecc3a0",
  "role": "Employee",
  "isActive": true
}
```

Expected: `200 OK`.

### Deactivate an employee

`DELETE /api/employees/{id}` - Admin only

Expected: `204 No Content`. This is a soft deactivation; historical data remains.

## 6. Department endpoints

### Read departments

- `GET /api/departments` - Admin or Manager
- `GET /api/departments/{id}` - Admin or Manager

### Create a department

`POST /api/departments` - Admin only

```json
{
  "name": "Quality Assurance",
  "description": "Quality engineering and testing"
}
```

Expected: `201 Created`.

### Update a department

`PUT /api/departments/{id}` - Admin only

```json
{
  "name": "Quality Assurance",
  "description": "Quality engineering and automated testing",
  "managerEmployeeId": null,
  "isActive": true
}
```

Expected: `200 OK`. A manager ID, when supplied, must reference an active Manager
in the same department.

### Deactivate a department

`DELETE /api/departments/{id}` - Admin only

Expected: `204 No Content`. A department with active employees returns
`409 Conflict`.

## 7. Leave-type endpoints

### Read leave types

- `GET /api/leave-types` - any authenticated user
- `GET /api/leave-types/{id}` - any authenticated user

### Create a leave type

`POST /api/leave-types` - Admin only

```json
{
  "name": "Personal Leave",
  "description": "Leave for personal matters",
  "defaultAnnualDays": 5
}
```

Expected: `201 Created`. Current-year balances are created for active employees.

### Update a leave type

`PUT /api/leave-types/{id}` - Admin only

```json
{
  "name": "Personal Leave",
  "description": "Updated personal leave policy",
  "defaultAnnualDays": 6,
  "isActive": true
}
```

Expected: `200 OK`.

### Deactivate a leave type

`DELETE /api/leave-types/{id}` - Admin only

Expected: `204 No Content`.

## 8. Expected authorization checks

| Test | Expected result |
|---|---|
| Protected endpoint without token | `401 Unauthorized` |
| Invalid email/password | `401 Unauthorized` |
| Employee calls an Admin endpoint | `403 Forbidden` |
| Manager calls an Admin write endpoint | `403 Forbidden` |
| Duplicate email or employee number | `409 Conflict` |
| Unknown resource ID | `404 Not Found` |
| Reuse token after logout | `401 Unauthorized` |
| Missing or invalid request fields | `400 Bad Request` |

Use a new unique email, employee number, and department/leave-type name each time
you repeat creation tests.

## Leave requests, balances, and reports
- GET /api/leave-balances (authenticated employee)
- GET/POST /api/leave-requests; PUT /api/leave-requests/{id}/cancel
- PUT /api/leave-requests/{id}/approve and /reject (Manager)
- GET /api/reports/leave-summary?year=2026
- GET /api/reports/department-leaves?year=2026
- GET /api/reports/monthly-leaves?year=2026
POST leave request requires Idempotency-Key header and body { leaveTypeId, startDate, endDate, reason }. Dates are inclusive weekdays; balance and overlapping active requests are validated.
