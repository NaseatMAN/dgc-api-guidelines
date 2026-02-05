# Git Workflow Guidelines

This document defines the standard Git workflow for the DGC project. Adhering to these practices ensures high code quality, clear history, and efficient collaboration.

## 1. Branching Strategy

We use a modified **GitHub Flow** for our development process.

### 1.1 Core Branches

- **`main`**: Production environment. Only stable, tested, and approved code.

- **`staging`**: Pre-production environment. Used for final QA and smoke testing before production.

- **`uat`**: User Acceptance Testing environment. Used for stakeholder validation.

- **`dev`**: The primary integration branch. All features and bug fixes are merged here first.



### 1.2 Supporting Branches

- **`feature/*`**: Used for new features. Branch off from `dev`.

- **`bugfix/*`**: Used for fixing bugs in `dev`. Branch off from `dev`.

- **`hotfix/*`**: Used for urgent production fixes. Branch off from `main`.

- **`release/*`**: Used for preparing a new deployment through the environments.

### 1.3 Naming Convention

Branches should be named using the following pattern:
`type/task-id-description`

| Type | Description | Example |
| :--- | :--- | :--- |
| `feature` | New functionality | `feature/DGC-101-add-user-auth` |
| `bugfix` | Resolving an issue | `bugfix/DGC-202-fix-login-error` |
| `hotfix` | Critical production fix | `hotfix/DGC-303-security-patch` |
| `chore` | Maintenance tasks | `chore/DGC-404-update-dependencies` |

---

## 2. Commit Message Standards

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification.

### 2.1 Format

`<type>(<scope>): <description>`

### 2.2 Types

- `feat`: A new feature.
- `fix`: A bug fix.
- `docs`: Documentation changes.
- `style`: Formatting, missing semi-colons, etc.; no code change.
- `refactor`: Refactoring production code.
- `test`: Adding missing tests, refactoring tests; no production code change.
- `chore`: Updating build tasks, package manager configs, etc.; no production code change.

### 2.3 Guidelines

- Use the **imperative mood** in the description ("add feature" instead of "added feature").
- Do not capitalize the first letter of the description.
- Do not end the description with a period.
- Keep the subject line under 50 characters.

---

## 3. Pull Request (PR) Process

### 3.1 Creating a PR

1. **Sync with Remote:** Ensure your branch is up to date with `dev`.

   ```bash
   git checkout dev
   git pull origin dev
   git checkout feature/your-branch
   git merge dev
   ```

2. **Atomic Commits:** Ensure your commits are small and focused.
3. **PR Template:** Fill out the PR description thoroughly:
    - **What:** Summary of changes.
    - **Why:** Link to the Jira/Task ID.
    - **How:** High-level technical approach.
    - **Testing:** Evidence of successful unit/integration tests.

### 3.2 Code Review

- All PRs require at least **one approved review** from a senior or peer.
- Address all comments and resolve conversations before merging.
- Use "Squash and Merge" to keep the history of the target branch clean.

### 3.3 Definition of Done (DoD)

A task is considered "Done" when:

- [ ] Code follows project standards and linting rules.
- [ ] Unit tests are written and passing.
- [ ] Documentation is updated (if applicable).
- [ ] PR is reviewed and approved.
- [ ] CI/CD pipeline passes.

---

## 4. Useful Commands

### 4.1 Feature Workflow

```bash
# Start a new feature
git checkout dev
git pull origin dev
git checkout -b feature/DGC-XXX-my-feature

# Commit changes
git add .
git commit -m "feat(api): add new endpoint for orders"

# Push and create PR
git push origin feature/DGC-XXX-my-feature
```

### 4.2 Handling Conflicts

```bash
git checkout dev
git pull origin dev
git checkout feature/your-branch
git merge dev
# Resolve conflicts in your editor, then:
git add <conflicted-files>
git commit -m "chore: merge dev into feature branch"
```
