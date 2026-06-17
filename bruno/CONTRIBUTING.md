# Bruno Collection — Conventions

## Structure

```
bruno/
├── {MODULE}/          ← UPPERCASE, one folder per module
│   └── {Resource}/   ← PascalCase, by aggregate or functional group
│       ├── folder.yml
│       └── {Resource}-{Verb}.yml
├── environments/
│   └── Local.yml
└── opencollection.yml
```

### Module folders

Named after the Atlas module in UPPERCASE: `IDENTITY`, `PARTY`, `PLATFORM`, `OUTBOX`, `STAFF`, `BUILDING_BLOCKS`, `HEALTH`.

### Resource subfolders

PascalCase, named after the aggregate or the functional group the requests belong to:

| Type | Example |
|---|---|
| Aggregate | `Persons/`, `Roles/`, `Invitations/` |
| Functional group | `Auth/`, `Session/` |
| Integration service | `Resend/` (inside `BUILDING_BLOCKS/`) |

For OUTBOX: one subfolder per source module — `Identity/`, `Staff/`, etc. Cross-module or generic requests stay at the `OUTBOX/` root level.

---

## File naming

```
{Resource}-{Verb}.yml
```

`Resource` = the subfolder name. `Verb` = the operation (see list below).  
Both the **file name** and the `info.name` field inside the file must follow this pattern — Bruno shows `info.name` in the tab, so it must be unambiguous when multiple tabs are open.

### Standard verbs

| Verb | HTTP | When |
|---|---|---|
| `List` | GET | Collection — no required filter |
| `GetById` | GET | Single resource by ID |
| `Create` | POST | Create |
| `Update` | PUT/PATCH | Full or partial update |
| `Delete` | DELETE | Hard delete |
| `Deactivate` | DELETE/PATCH | Soft delete |
| `Lookup` | GET | Lightweight list for dropdowns |

### Variant suffix

When the same verb exists in multiple flavours, append a descriptor:

```
Entries-List.yml
Entries-ListByEntityType.yml
Entries-ListByEntityTypeAndAction.yml
Messages-List.yml
Messages-ListByDateRange.yml
Persons-Create.yml
Persons-CreateWithAddress.yml
```

### Non-CRUD operations

For requests that don't map to standard CRUD (outbox processing, auth flows, direct integrations):  
use a descriptive PascalCase verb that names the action:

```
Auth-FakeLogin.yml
Auth-GetXsrf.yml
Resend-SendEmailDirectly.yml
```

---

## Environment variables

Defined in `environments/Local.yml`. Current variables:

| Variable | Value |
|---|---|
| `bff_api_prefix` | `https://localhost:7106` |
| `xsrf_token` | set at runtime by `Auth-GetXsrf.yml` |

**Never commit real secrets** (API keys, tokens, passwords) to environment files.  
Use `<placeholder>` syntax in request files for values that must be filled manually.

---

## Adding a new endpoint

1. Open (or create) the subfolder for the aggregate: `{MODULE}/{Resource}/`
2. Create `{Resource}-{Verb}.yml` following the template below
3. Set `info.name` to match the file name (without `.yml`)
4. Set `seq` to the next available number in that folder

### Template

GET (sem body):
```yaml
info:
  name: Persons-List
  type: http
  seq: 1

http:
  method: get
  url: "{{bff_api_prefix}}/bff/v1/party/persons"
  auth: inherit

settings:
  encodeUrl: false
  timeout: 0
  followRedirects: true
  maxRedirects: 5
```

POST/PUT/PATCH (com body):
```yaml
info:
  name: Persons-Create
  type: http
  seq: 3

http:
  method: post
  url: "{{bff_api_prefix}}/bff/v1/party/persons"
  headers:
    - name: Content-Type
      value: application/json
    - name: X-XSRF-TOKEN
      value: "{{xsrf_token}}"
  body:
    type: json
    data: |-
      {
        "field": "value"
      }
  auth: inherit

settings:
  encodeUrl: false
  timeout: 0
  followRedirects: true
  maxRedirects: 5
```

**Never add a `Cookie` header** — the Bruno session cookie is set automatically after `Auth-FakeLogin` and is managed by Bruno.
