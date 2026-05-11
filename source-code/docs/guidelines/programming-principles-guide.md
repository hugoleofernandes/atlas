# Programming Principles for AI
A deterministic guideline for AI to produce simple, explicit, human‑readable code.

---

## 1. Simplicity First (The Simplest Solution Is Always the Best)

### Definition
The AI must always choose the simplest possible solution that solves the problem correctly.  
“Simplicity” is defined from a **human perspective**, not from the AI’s internal optimization.

### What “Simple” Means
A solution is considered simple when it is:

- Easy to read — a human developer can understand it quickly.
- Easy to explain — the logic can be described in plain language without effort.
- Easy to debug — minimal hidden behavior, minimal magic, minimal indirection.
- Easy to maintain — predictable structure, minimal cleverness, minimal abstraction unless necessary.
- Easy to extend — the next developer can safely modify it without fear.

### AI Interpretation Rules
- Prefer straightforward code over “smart” or “clever” code.
- Prefer clear control flow over compact or overly abstract patterns.
- Prefer standard language features over advanced constructs unless required.
- Prefer explicit naming over short or cryptic identifiers.
- Prefer fewer moving parts over layered abstractions.

XXXXXXXXXXXX
### Additional Rules for Simplicity
- Compact expressions are allowed **only when they remain explicit and readable**.
- Pattern matching is allowed when it improves clarity without hiding behavior.
- One‑line expressions are acceptable when they make the intent clearer, not more cryptic.
XXXXXXXXXXXX

### Examples of Simplicity
- A plain `foreach` loop is simpler than a complex LINQ chain.
- A small, well‑named method is simpler than an inline block with multiple responsibilities.
- A direct conditional is simpler than pattern‑matching gymnastics when both solve the same problem.

### Anti‑Patterns
- Over‑engineering.
- Premature abstraction.
- Clever tricks that reduce readability.
- Hidden behavior (implicit conventions, magic defaults, side effects).

### Workflow Simplicity Rules

- Workflows must remain **linear and readable**; avoid splitting steps into private methods unless it significantly improves clarity.
- Do **not** create helper methods that merely wrap a single obvious call (e.g., `AppendAuditLogsAsync`, `AddOutboxMessageAsync`).
- Avoid boilerplate: repeating the same private method across workflows violates simplicity.
- Prefer inline, explicit, compact expressions when they are clear and readable.
- Only extract a method when:
  - the logic is complex,
  - the name adds real clarity,
  - or the code is reused across multiple workflows.


---

## 2. Explicit Is Better Than Implicit

### Definition
The AI must always prefer explicit behavior, explicit structure, and explicit intent.  
Implicit behavior assumes knowledge of conventions or hidden rules — which violates Principle #1.

### Why Explicitness Matters
Explicit code:

- Is easier to understand because nothing is hidden.
- Is easier to debug because behavior is visible.
- Is easier to maintain because intent is clear.
- Is safer because it avoids assumptions.

### AI Interpretation Rules
- Never rely on hidden conventions when explicit code is possible.
- Always show the full behavior instead of assuming defaults.
- Make dependencies visible (constructor injection, parameters, explicit configuration).
- Make data transformations visible (no silent conversions, no magic mapping).
- Make control flow visible (no implicit branching, no hidden side effects).

### Additional Explicitness Rules
- **Generic type‑based methods are considered explicit** when the type parameter communicates the intent clearly.  
  Examples:  
  - `GetEvent<T>()`  
  - `GetPolicy<T>()`  
  - `GetHandler<T>()`
- Using `is { }` for null checking is explicit and acceptable.
- Explicitness includes **narrative naming**: names must describe exactly what is happening, as if summarizing the behavior for a human reader.
- Avoid generic verbs like “Handle”, “Process”, “Manage”, “Do”, “Execute”.  
  Prefer narrative names such as:  
  - `AddOutboxMessageForUserCreatedFromInvitationAsync`  
  - `AppendAuditLogsAsync`  
  - `PersistChangesAsync`

### Examples of Explicitness
- Explicitly passing dependencies instead of using service locators.
- Explicitly naming variables instead of relying on context.
- Explicitly handling edge cases instead of assuming they won’t happen.
- Explicitly returning values instead of relying on implicit returns.

### Use Case Naming Rules

- A use case class must have a **narrative name** that clearly expresses its purpose.  
  Examples:
  - `ResolveAccessUseCase`
  - `CreateUserUseCase`
  - `ChangePasswordUseCase`

- The main method of a use case may be named `ExecuteAsync` **only when**:
  - It comes from a shared contract such as `IUseCase<TCommand, TOutput>`.
  - The class name already provides the narrative meaning.
  - The combination `ClassName.ExecuteAsync` is fully explicit for a human reader.

- In this pattern, the class name carries the narrative intent, and `ExecuteAsync` is treated as a **standardized execution verb**, not a generic or ambiguous name.

- Outside of well‑defined contracts like `IUseCase`, generic verbs such as `Execute`, `Handle`, `Process`, `Do`, `Manage` remain prohibited.  
  In these cases, the method must be narrative (e.g., `ResolveAccessAsync`, `CreateUserAsync`).


### Anti‑Patterns
- Implicit conventions that require tribal knowledge.
- Hidden behavior inside frameworks or helpers.
- Magic configuration.
- Overuse of defaults that hide intent.

---

## Synthesis: How Principle #2 Reinforces Principle #1
Explicit code is simpler because:

- It removes ambiguity.
- It avoids assumptions.
- It makes the logic visible.
- It reduces cognitive load.
- It prevents debugging surprises.

Therefore:

**Explicitness is a direct extension of simplicity.**

---

### Explicitness vs Boilerplate

- Explicit code does **not** mean creating extra methods.
- Explicitness is achieved through **clear inline expressions**, not through unnecessary indirection.
- A single readable line (e.g., `if (output.GetEvent<T>() is { } evt)`) is preferred over a private method that hides the same behavior.

---

## 3. Standardize Early, Scale Faster

### Definition
Organization must happen early.  
The sooner standards are defined and applied, the sooner the project becomes scalable, predictable, and easy to evolve.  
If no standard exists, the AI must create one that is simple, explicit, and aligned with existing guidelines.

### Why Early Standardization Matters
Delaying organization leads to permanent disorder.  
Early standards prevent chaos and ensure that every new piece of code fits naturally into the project.

Standards accelerate productivity because they:

- Reduce ambiguity.
- Reduce cognitive load.
- Prevent structural drift.
- Enable consistent decision‑making.
- Allow the project to grow without losing coherence.

### AI Interpretation Rules
- If a standard exists → **follow it strictly**.
- If a guideline exists → **consult it before producing anything**.
- If no standard exists → **propose a simple, explicit, consistent standard**.
- Never improvise folder structure, naming, or architecture.
- Never leave organization for later.
- Always enforce consistency across files, modules, layers, and contexts.

### Applications
- Folder structure must follow a defined pattern.
- File names must follow a defined pattern.
- Class, method, and variable names must follow a defined pattern.
- Architecture must follow a defined pattern.
- Test organization must follow a defined pattern.
- Documentation must follow a defined pattern.
- New modules must align with existing conventions.

### Anti‑Patterns
- Creating files in arbitrary locations.
- Naming without criteria.
- Mixing styles or conventions.
- Producing code without checking existing guidelines.
- Leaving “TODO: organize later”.
- Inventing structure on the fly.

### Guideline Dependency
Standards come from guidelines.  
Therefore, the AI must always:

1. Check existing guidelines quickly and efficiently.  
2. Apply them consistently.  
3. Suggest new guidelines when a missing standard is identified.

Early standardization enables faster scaling — in code, architecture, and productivity.

## Workflow Structure (Mandatory Pattern)

A workflow is a **linear sequence of explicit steps**.  
Workflows are allowed to contain multiple responsibilities **as long as they represent a clear, ordered business flow**.

A workflow must follow this structure:

1. **Validate input**  
2. **Execute the use case**  
3. **Extract domain events**  
4. **Handle domain events explicitly**  
5. **Add audit logs**  
6. **Persist changes**  
7. **Return the final result**

Workflows must be readable top‑to‑bottom like a narrative.  
Each step must be visible and named descriptively.  
Workflows must not hide steps behind vague helper methods.

---



## 4. Consistency Over Creativity

### Definition
Consistency is more valuable than creativity in software development.  
Creative variations introduce noise, ambiguity, and cognitive load.  
Consistency ensures that every part of the system feels familiar, predictable, and aligned with established standards.

The AI must always prioritize consistency over inventing new patterns, styles, or structures.

### Why Consistency Matters
Consistency:

- Reduces the number of decisions a developer must make.
- Makes the codebase feel unified, regardless of who wrote each part.
- Improves readability and onboarding.
- Prevents fragmentation of styles, patterns, and architectural approaches.
- Ensures that standards and guidelines remain effective over time.

Creativity, when applied to foundational aspects of code, often leads to divergence and confusion.

### AI Interpretation Rules
- Follow existing patterns before creating new ones.
- Match the naming style, folder structure, and architectural conventions already in place.
- Use the same approach to similar problems across the codebase.
- Avoid introducing new abstractions, styles, or patterns unless absolutely necessary.
- Prefer predictable solutions over novel ones.
- Ensure that new code “feels like” the rest of the project.

### Additional Consistency Rules
- Narrative naming must be applied consistently across all workflows, handlers, services, and modules.
- Generic type‑based access patterns (e.g., `GetEvent<T>()`) must be used consistently across the codebase.
- Workflow steps must always follow the same order and naming conventions.

### Applications
- Use the same naming conventions across all modules.
- Follow the same architectural layering and boundaries.
- Apply the same error‑handling strategy everywhere.
- Structure tests in the same way across bounded contexts.
- Use consistent formatting, spacing, and file organization.
- Reuse existing patterns for commands, queries, events, and workflows.

### Anti‑Patterns
- Inventing new patterns when an existing one works.
- Mixing different architectural styles in the same project.
- Using different naming conventions for similar concepts.
- Creating unique folder structures for each module.
- Solving similar problems with different approaches.
- Introducing unnecessary creativity that reduces predictability.

### Relationship to Previous Principles
Consistency reinforces:

- **Simplicity** — because predictable code is easier to understand.
- **Explicitness** — because consistent patterns make intent clearer.
- **Standardization** — because standards only work when applied uniformly.

Consistency transforms guidelines into discipline and discipline into scalable architecture.



5. Pragmatism Over Politeness
Definition
The AI must prioritize pragmatism, clarity, and efficiency over politeness or conversational embellishment.
The user’s time is scarce, and responses must be direct, structured, and immediately useful.

Why This Matters
Polite or verbose answers waste time and add no value.
Objective, concise communication accelerates understanding and decision‑making.

Pragmatic responses:

Reduce cognitive load

Save time

Improve clarity

Increase productivity

Avoid unnecessary wording

AI Interpretation Rules
Always answer directly, objectively, and logically.

Do not add filler, politeness, or conversational fluff.

Do not explain more than necessary unless the user explicitly asks.

Prioritize clarity, brevity, and structure.

Present information in a logical sequence.

Use lists, steps, or ordered reasoning when appropriate.

Avoid rhetorical questions, greetings, or emotional tone.

Never hide the core answer behind explanations — answer first, explain after (only if needed).

When in doubt, choose the shortest correct answer.

Applications
Provide the solution immediately, then optional context.

Use structured formatting to reduce reading time.

Avoid storytelling, metaphors, or unnecessary examples.

Keep sentences short and direct.

Use technical precision instead of conversational tone.

Anti‑Patterns
Long introductions.

Polite fillers (“hope you’re well”, “let me explain”).

Over‑explanation.

Conversational fluff.

Emotional tone.

Indirect answers.

Circular reasoning.

Relationship to Other Principles
Supports Simplicity by reducing noise.

Supports Explicitness by removing ambiguity.

Supports Consistency by enforcing a predictable communication style.

Supports Standardization by defining a clear response pattern.

### Response Focus Rules
- The AI must **prioritize problems over compliments**.
- Do not list what is correct unless the user explicitly asks for a full review.
- Default behavior: **only point out what is wrong, missing, ambiguous, or inconsistent**.
- Start responses with **issues, violations, or concrete improvements**, not with validation or praise.
- Avoid phrases like “o que está certo”, “está muito bom”, “excelente”, unless explicitly requested.
- Assume the user already sabe o que está certo — ele quer saber **onde está fraco**.

When reviewing code or documents, the AI must:
- **Skip positive feedback by default.**
- **Go straight to the problems and suggestions.**

### Correction Delivery Rules

When reviewing code, files, or architecture, the AI must follow these rules:

1. **Always focus on what is wrong, missing, inconsistent, or unclear.**
2. **Never list what is correct unless explicitly requested.**
3. **Every correction must include:**
   - The **exact file name** where the correction applies.
   - The **expected folder or module** where the file is located (when known).
   - The **corrected version of the code**, shown in full or in the modified section.
4. Corrections must be **direct, concise, and immediately actionable**.
5. The AI must not use praise, validation, or politeness. Only objective analysis.
6. The AI must always present corrections in the order:
   - Problem  
   - Why it violates the guide  
   - Corrected code  
   - File name / location  

   ### Correction Delivery Rules

When reviewing code, files, or architecture, the AI must follow these rules:

1. Focus **only** on what is wrong, missing, inconsistent, or unclear.
2. Do **not** list what is correct unless explicitly requested.
3. Every correction must include:
   - The **exact file name** where the correction applies.
   - The **expected folder or module** where the file is located (when known).
   - The **corrected version of the code**, shown in full or only the modified section.
4. Corrections must be **direct, concise, and immediately actionable**.
5. Avoid praise, validation, or politeness. Only objective analysis.
6. Always present corrections in this order:
   - Problem  
   - Why it violates the guide  
   - Corrected code  
   - File name / location  

   ### Pragmatic Workflow Rules

- The AI must avoid generating boilerplate in workflows.
- The AI must prefer the simplest readable inline form over method extraction.
- The AI must not propose abstractions that increase code volume without increasing clarity.
