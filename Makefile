PROJECT=atlas
COMPOSE=docker compose -p $(PROJECT) -f infrastructure/docker-compose.yml -f infrastructure/docker-compose.dev.yml

SRC=source-code/src
API=$(SRC)/Atlas.API
IDENTITY=$(SRC)/Atlas.Identity.Infrastructure
STAFF=$(SRC)/Atlas.Staff.Infrastructure

IDENTITY_CONTEXT=IdentityDbContext
STAFF_CONTEXT=StaffDbContext

# =========================================
# DOCKER
# =========================================

up:
	$(COMPOSE) up --build -d

down:
	$(COMPOSE) down

logs:
	$(COMPOSE) logs -f

ps:
	$(COMPOSE) ps

reset:
	$(COMPOSE) down -v

# =========================================
# IDENTITY
# =========================================

migrate-identity:
	dotnet ef migrations add $(name) \
	--project $(IDENTITY) \
	--startup-project $(API) \
	--context $(IDENTITY_CONTEXT) \
	--output-dir Persistence/Migrations

update-identity:
	dotnet ef database update \
	--project $(IDENTITY) \
	--startup-project $(API) \
	--context $(IDENTITY_CONTEXT)

# =========================================
# STAFF
# =========================================

migrate-staff:
	dotnet ef migrations add $(name) \
	--project $(STAFF) \
	--startup-project $(API) \
	--context $(STAFF_CONTEXT) \
	--output-dir Persistence/Migrations

update-staff:
	dotnet ef database update \
	--project $(STAFF) \
	--startup-project $(API) \
	--context $(STAFF_CONTEXT)

# =========================================
# GLOBAL
# =========================================

migrate-all:
	make migrate-identity name=$(name)_Identity
	make migrate-staff name=$(name)_Staff

update-all:
	make update-identity
	make update-staff

# sample usage:

# test:
# 	dotnet test

# build:
# 	dotnet build

# make migrate-identity name=InitialIdentity
# make migrate-staff name=InitialStaff

# make migrate-all name=Initial
# make update-all


# =========================================
# DOCS (DOCFX)
# =========================================

SHELL := powershell.exe
.SHELLFLAGS := -NoProfile -Command

DOCS_DIR=source-code/docs

docs-clean:
	if (Test-Path "$(DOCS_DIR)\api") { Remove-Item -Recurse -Force "$(DOCS_DIR)\api" }
	if (Test-Path "$(DOCS_DIR)\_site") { Remove-Item -Recurse -Force "$(DOCS_DIR)\_site" }

docs-metadata:
	cd $(DOCS_DIR); docfx metadata docfx.json

docs-build:
	cd $(DOCS_DIR); docfx build docfx.json

docs-serve:
	cd $(DOCS_DIR); docfx serve _site --port 9000

docs: docs-clean docs-metadata docs-build

docs-v: docs-clean
	cd $(DOCS_DIR); docfx metadata docfx.json --logLevel verbose
	cd $(DOCS_DIR); docfx build docfx.json --logLevel verbose