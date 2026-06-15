# Documentation Maintenance

## Rule

After every feature implementation or behavior change, verify whether any `.claude/docs/` file needs updating before considering the task complete.

## When to Check

✅ New pattern introduced → update the relevant doc or create a new one
✅ Existing behavior changed (method removed, interface renamed, seeder logic changed) → update the doc
✅ New architectural concept added (new layer, new module, new cross-cutting mechanism) → create a new doc + add to `CLAUDE.md`
✅ A rule documented with ❌ no longer applies → remove or correct it
❌ Never leave a doc that contradicts the current code — stale rules are worse than no rules

## Checklist — Run After Every Implementation

1. What files did I change or create?
2. Do any `.claude/docs/` files reference those areas?
3. Does the implementation introduce a pattern not yet documented?
4. Does the implementation remove or replace something currently documented?

If any answer is yes → update the doc before finishing.

## Which Doc to Update

| What changed | Doc to update |
|---|---|
| New permission or verb | `permissions.md` |
| New entity type | `entity-types.md` |
| Seeder pattern change | `seeding.md` |
| New endpoint convention | `endpoints.md` |
| Repository interface location changed | `repositories.md` |
| New domain event / integration event | `domain-events.md` |
| New handler pipeline decorator | `handler-invoker.md` |
| New resource file added | `localization-resources.md` |
| New cross-cutting architectural concept | new file + add to `CLAUDE.md` |
