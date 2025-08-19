# GitHub Repository Information

## Repository Details
- **Repository Name**: FDX.trading
- **Organization**: foodXchange
- **Repository URL**: https://github.com/foodXchange/FDX.trading
- **Visibility**: Public
- **License**: MIT
- **Primary Branch**: main
- **Created**: January 18, 2025

## Repository URLs
- **HTTPS Clone**: https://github.com/foodXchange/FDX.trading.git
- **SSH Clone**: git@github.com:foodXchange/FDX.trading.git
- **Web URL**: https://github.com/foodXchange/FDX.trading

## Git CLI Commands

### Cloning the Repository
```bash
# Using GitHub CLI
gh repo clone foodXchange/FDX.trading

# Using Git HTTPS
git clone https://github.com/foodXchange/FDX.trading.git

# Using Git SSH
git clone git@github.com:foodXchange/FDX.trading.git
```

### Setting Up Local Repository
```bash
# Navigate to project directory
cd FDX.trading

# Check remote configuration
git remote -v

# Set upstream (if forked)
git remote add upstream https://github.com/foodXchange/FDX.trading.git
```

### Basic Git Operations
```bash
# Check status
git status

# Pull latest changes
git pull origin main

# Create new branch
git checkout -b feature/branch-name

# Stage changes
git add .

# Commit changes
git commit -m "Your commit message"

# Push to GitHub
git push origin branch-name
```

### GitHub CLI Commands
```bash
# Install GitHub CLI (if not installed)
# Windows: winget install --id GitHub.cli
# Mac: brew install gh
# Linux: See https://github.com/cli/cli/blob/trunk/docs/install_linux.md

# Authenticate GitHub CLI
gh auth login

# View repository
gh repo view foodXchange/FDX.trading

# Create issue
gh issue create --title "Issue title" --body "Issue description"

# Create pull request
gh pr create --title "PR title" --body "PR description"

# List pull requests
gh pr list

# Check workflow runs
gh run list
```

### Branch Management
```bash
# List all branches
git branch -a

# Switch branches
git checkout branch-name

# Create and switch to new branch
git checkout -b new-branch-name

# Delete local branch
git branch -d branch-name

# Delete remote branch
git push origin --delete branch-name
```

### Syncing Fork (if applicable)
```bash
# Add upstream remote
git remote add upstream https://github.com/foodXchange/FDX.trading.git

# Fetch upstream
git fetch upstream

# Merge upstream changes
git checkout main
git merge upstream/main

# Push to your fork
git push origin main
```

## Repository Structure
```
FDX.trading/
├── .github/              # GitHub specific files (actions, templates)
├── Components/           # Blazor components
├── Data/                # Entity models
├── Database/            # SQL scripts and migrations
├── Documentation/       # Project documentation
│   ├── Credentials/     # This file location
│   ├── Database/        # Schema documentation
│   ├── ProjectPlan/     # Vision and roadmap
│   ├── Sessions/        # Development logs
│   └── TechStack/       # Technology stack
├── Services/            # Business logic
├── wwwroot/            # Static files
├── .gitignore          # Git ignore rules
├── LICENSE             # MIT License
├── README.md           # Project readme
└── CONTRIBUTING.md     # Contribution guidelines
```

## Commit Message Convention
```
Type: Brief description

- Add: for new features
- Fix: for bug fixes
- Update: for updates to existing features
- Remove: for removing features
- Refactor: for code refactoring
- Docs: for documentation
- Test: for tests
- Style: for formatting
- Chore: for maintenance
```

### Example Commit Messages
```bash
git commit -m "Add: User authentication service"
git commit -m "Fix: Database connection timeout issue"
git commit -m "Update: Company profile validation rules"
git commit -m "Docs: Add API documentation"
```

## Pull Request Process
1. Fork the repository (if not a direct contributor)
2. Create a feature branch from `main`
3. Make your changes
4. Commit with descriptive messages
5. Push to your branch
6. Create a Pull Request via GitHub or CLI
7. Wait for review and address feedback
8. Merge after approval

## GitHub Actions (Future)
```yaml
# Planned workflows
- CI/CD Pipeline
- Automated Testing
- Code Quality Checks
- Security Scanning
- Documentation Build
```

## Repository Settings
- **Default Branch**: main
- **Branch Protection**: (To be configured)
  - Require pull request reviews
  - Require status checks
  - Require up-to-date branches
- **Issues**: Enabled
- **Projects**: Enabled
- **Wiki**: Disabled (using Documentation folder)
- **Discussions**: (To be enabled)

## Important Files
| File | Purpose |
|------|---------|
| README.md | Project overview and setup instructions |
| LICENSE | MIT License terms |
| CONTRIBUTING.md | Guidelines for contributors |
| .gitignore | Files to exclude from version control |
| FDX.trading.csproj | Project configuration |
| appsettings.json | Application settings |

## Security Notes
- **Never commit**: 
  - Passwords or API keys
  - Connection strings with credentials
  - Personal access tokens
  - Private certificates
  - Production secrets

- **Use for secrets**:
  - GitHub Secrets for Actions
  - Azure Key Vault for production
  - Environment variables locally
  - appsettings.Production.json (gitignored)

## Collaboration
- **Code Owners**: @foodXchange team
- **Main Contributor**: Udi Stryk (foodz-x@hotmail.com)
- **Issue Labels**: bug, enhancement, documentation, help wanted, good first issue
- **Review Required**: Yes, for main branch

## Useful Links
- **Repository**: https://github.com/foodXchange/FDX.trading
- **Issues**: https://github.com/foodXchange/FDX.trading/issues
- **Pull Requests**: https://github.com/foodXchange/FDX.trading/pulls
- **Actions**: https://github.com/foodXchange/FDX.trading/actions
- **Projects**: https://github.com/foodXchange/FDX.trading/projects

## Quick Start for New Contributors
```bash
# 1. Clone the repository
gh repo clone foodXchange/FDX.trading

# 2. Navigate to project
cd FDX.trading

# 3. Install dependencies
dotnet restore

# 4. Create feature branch
git checkout -b feature/your-feature

# 5. Make changes and commit
git add .
git commit -m "Add: Your feature description"

# 6. Push and create PR
git push origin feature/your-feature
gh pr create
```

---

**Last Updated**: January 18, 2025
**Repository Status**: Active Development
**Current Version**: 0.1.0-alpha