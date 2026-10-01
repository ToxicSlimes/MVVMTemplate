# Screen Spec

Canonical layout contract between `Packages/ui-design` and Unity UI Toolkit.

- Edit `*.screen.json` files (or generate via AI using `PROMPT.md`).
- Run `cd Packages/ui-design/npm && npm run validate && npm run export`.
- In Unity: **Tools → UI Toolkit → Import All Screen Specs**.

Do not hand-edit generated UXML; change the spec and re-import.
