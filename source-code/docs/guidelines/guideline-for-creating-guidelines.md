# Guideline for Creating Guidelines

## Language Rule
All guidelines must be written in English.

---

## Purpose
This document defines the standard structure, tone, boundaries, and AI‑oriented rules for creating new guidelines within this project.  
Its goal is to ensure **consistency, clarity, determinism, and professional quality**, especially when guidelines are generated or extended by AI.

---

## Principles
1. Guidelines must be short, structured, and objective  
2. Guidelines must define boundaries (what they cover and what they do NOT cover)  
3. Guidelines must avoid redundancy with existing documents  
4. Guidelines must reflect the ubiquitous language of the project  
5. Guidelines must be actionable, not theoretical  
6. Guidelines must be scannable (sections, lists, separators)  
7. Guidelines must be consistent with all other guidelines  
8. Guidelines must be deterministic and unambiguous for AI interpretation  
9. Guidelines must be easy to validate and apply  

---

## Required Structure

Every guideline must follow this structure:

### 1. Title
Clear, descriptive, aligned with the domain.

### 2. Language Rule
State that all documentation must be written in English.

### 3. Purpose
Explain why the guideline exists and what problem it solves.

### 4. Principles
List the core principles that guide the topic.

### 5. What to Do
Actionable rules, steps, or expectations.

### 6. What NOT to Do
Explicit anti‑patterns to avoid confusion.

### 7. Examples
Concrete examples that illustrate correct usage.

### 8. Checklist
A quick validation list to ensure compliance.

### 9. Anti‑Patterns
Common mistakes and what to avoid.

### 10. AI Usage Rules
How AI should generate or assist with content related to this guideline.

---

## Tone and Style Requirements
- Use short sections  
- Use clear headers  
- Use lists instead of paragraphs  
- Avoid long explanations  
- Avoid academic language  
- Avoid ambiguity  
- Use consistent terminology across all guidelines  
- Use separators (`---`) between major sections  
- Keep the document scannable and minimalistic  
- Prefer imperative voice (“Do”, “Avoid”, “Ensure”)  

---

## Boundaries of a Guideline

Each guideline must explicitly state:

### This guideline covers:
- The scope of the topic  
- The responsibilities of the layer or concept  
- The rules and expectations  

### This guideline does NOT cover:
- Topics handled by other guidelines  
- Implementation details  
- Infrastructure concerns  
- Framework‑specific behavior  

This prevents overlap and keeps each guideline focused.

---

## Cross‑Guideline Consistency Rules

When creating a new guideline, ensure:

- It does not duplicate content from:
  - Domain Design Playbook  
  - DDD Commenting Guidelines  
  - Documentation Guidelines  
  - Unit Testing Guidelines  
  - Application Testing Guidelines  

- Terminology matches existing documents  
- Examples follow the same style  
- Checklists follow the same pattern  
- Anti‑patterns follow the same format  
- The guideline fits naturally into the documentation ecosystem  

---

## AI Interpretation Rules

When an AI uses this guideline, it must:

- Interpret all rules as strict requirements  
- Prefer explicit instructions over inferred behavior  
- Resolve ambiguity conservatively and consistently  
- Prioritize consistency with existing guidelines over creativity  
- Never contradict:
  - Domain Design Playbook  
  - Testing Guidelines  
  - Documentation Guidelines  
  - This guideline  

- Always check whether a guideline already exists before creating a new one  
- Always use the ubiquitous language of the project  

---

## Conflict Resolution Rules

When two guidelines appear to conflict, the AI must resolve conflicts in this order:

1. Domain Design Playbook (highest authority)  
2. DDD Commenting Guidelines  
3. Documentation Guidelines  
4. Testing Guidelines (Domain + Application)  
5. Guideline for Creating Guidelines (this document)  
6. The new guideline being created  

The AI must choose the rule that:
- Preserves domain correctness  
- Preserves consistency across documents  
- Avoids redundancy  

---

## When to Create a New Guideline

Create a new guideline only when:

- The topic is large enough to justify its own document  
- The topic is reused across multiple modules  
- The topic has rules, boundaries, and anti‑patterns  
- The topic is not already covered elsewhere  

Do NOT create guidelines for:

- One‑off decisions  
- Implementation details  
- Temporary rules  
- Framework‑specific quirks  
- Topics that belong inside an existing guideline  

---

## When NOT to Create a Guideline (AI‑Specific)

The AI must NOT create a new guideline when:

- The topic is already covered by an existing guideline  
- The topic is too small and belongs inside another guideline  
- The topic is a one‑off decision or implementation detail  
- The topic is framework‑specific or temporary  
- The topic does not affect multiple modules or layers  
- The topic is not part of the ubiquitous language  

---

## AI Consistency Enforcement

When generating a new guideline, the AI must:

- Ensure section titles match the required structure  
- Ensure tone and style match existing guidelines  
- Ensure examples follow the same formatting conventions  
- Ensure checklists use the same checkbox style  
- Ensure anti‑patterns use the ❌ prefix  
- Ensure separators (`---`) are used between major sections  
- Ensure the guideline is scannable and minimalistic  
- Ensure no new terminology is introduced without justification  

---

## Example Template

[Guideline Title]
Language Rule
All documentation must be written in English.

Purpose
[Why this guideline exists]

Principles
[Principle 1]

[Principle 2]

[Principle 3]

What to Do
[Rule 1]

[Rule 2]

[Rule 3]

What NOT to Do
[Anti-rule 1]

[Anti-rule 2]

Examples
[Code or conceptual examples]

Checklist
[ ] Rule 1 followed

[ ] Rule 2 followed

[ ] Rule 3 followed

Anti-Patterns
❌ [Anti-pattern 1]
❌ [Anti-pattern 2]

AI Usage Rules
Always follow the structure above

Never generate redundant content

Always use the ubiquitous language

Always keep sections short and objective

---


---

## AI Validation Checklist

Before finalizing a guideline, the AI must verify:

- [ ] The guideline follows the required structure  
- [ ] The guideline does not duplicate existing content  
- [ ] The guideline uses consistent terminology  
- [ ] The guideline includes boundaries  
- [ ] The guideline includes examples  
- [ ] The guideline includes a checklist  
- [ ] The guideline includes anti‑patterns  
- [ ] The guideline includes AI usage rules  
- [ ] The guideline is short, objective, and scannable  

---

## Final Notes

A guideline must:
- Be easy to read  
- Be easy to apply  
- Be easy to validate  
- Be consistent with the rest of the documentation  

A guideline is not a tutorial — it is a **rulebook**.



## AI File Generation Rules (Markdown Output)

When generating a guideline as a `.md` file, the AI must:

### 1. Produce a Single, Continuous Markdown Document
- The entire guideline must be generated in one continuous Markdown block.
- No fragmented messages.
- No partial sections.
- No content spread across multiple responses.

### 2. Avoid Parser‑Breaking Syntax
The AI must avoid:
- Unescaped triple backticks (```).
- Nested code fences.
- Characters that break Markdown rendering in file‑generation environments.

### 3. Use Safe Code Blocks
All code examples must use the safe wrapper format:



<codeblock language="csharp">
...
</codeblock>


or


<codeblock language="markdown">
...
</codeblock>



Never use raw triple‑backtick fences inside file‑generation prompts.

### 4. Ensure the File Is Ready for Copy/Paste or Download
The generated `.md` must:
- Be directly usable as a file.
- Require zero manual fixing.
- Contain no placeholders that break formatting.
- Contain no invisible characters or malformed indentation.

### 5. Preserve Markdown Semantics
The AI must ensure:
- Headers use `#`, `##`, `###` consistently.
- Lists use `-` or `1.` consistently.
- Separators use `---` between major sections.
- No HTML or exotic Markdown unless explicitly required.

### 6. Never Include Execution‑Specific Metadata
The AI must not include:
- Tool instructions
- System messages
- Execution logs
- Internal reasoning
- Any content not intended to appear in the final `.md` file

### 7. Validate Before Output
Before finalizing the `.md` file, the AI must verify:

- [ ] The document is complete  
- [ ] The document is continuous  
- [ ] All code blocks use `<codeblock>` wrappers  
- [ ] No triple backticks exist  
- [ ] No parser‑breaking characters exist  
- [ ] The structure matches the Required Structure section  
- [ ] The tone matches the Tone and Style Requirements  
- [ ] The guideline is deterministic and unambiguous  
