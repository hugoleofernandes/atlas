# Guide for Writing Claude Documentation

These files are **not for humans**. They are instructions for Claude.

Claude already understands Clean Architecture, CQRS, DDD, FluentValidation, EF Core, Dapper, etc.
Do not explain concepts. Document **how this project applies them** and **what is forbidden**.

---

## What Claude needs

- Project-specific constraints it cannot infer from the code alone
- Rules that prevent mistakes even when a reference exists in the project
- Correct patterns with code — one canonical example per case
- Explicit prohibitions — what looks valid but is not

## What Claude does not need

- Definitions of known concepts ("FluentValidation is a library that...")
- History or motivation paragraphs ("we chose this because...")
- Summaries of what was just shown
- Long introductions before the first rule

---

## File Structure

```markdown
# Topic

## Rules
✅ short affirmative rule
✅ short affirmative rule
❌ short prohibition — one line, no elaboration needed
❌ short prohibition

## Pattern
[one canonical correct code example]

## Anti-patterns
[wrong code + one-line comment explaining why it's wrong]

## [Optional: How to add / Checklist]
[step list only when the sequence is non-obvious and matters]
```

---

## Writing Rules

**Lead with the rules block.** It is the most important section. Everything else supports it.

**One rule per line.** Do not combine two constraints into one sentence.

**✅ and ❌ are not decoration** — ✅ means "always do this", ❌ means "never do this, even when the project reference exists".

**Code examples must show both correct and wrong.** A correct example without a wrong counterpart leaves Claude guessing where the line is.

**If a rule has an exception, state it explicitly.** "Never X — except when Y" is better than a rule that Claude silently breaks when Y occurs.

**Omit optional context.** If Claude can infer something from the code, do not write it. Write only what Claude would get wrong without the doc.

---

## Length Target

- Rules block: 3–8 lines
- Pattern: 10–20 lines of code
- Anti-patterns: 5–15 lines of code
- Total file: under 60 lines

If a file needs more than 60 lines, it covers two topics — split it.
