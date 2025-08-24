#!/bin/bash
# Remove sensitive data from git history

echo "Creating backup branch..."
git checkout -b backup-before-clean-$(date +%Y%m%d-%H%M%S)
git checkout main

echo "Removing sensitive files from history..."
# Remove appsettings.json with passwords from history
git filter-branch --force --index-filter \
'git rm --cached --ignore-unmatch FoodX.Admin/appsettings.json' \
--prune-empty --tag-name-filter cat -- --all

# Clean up refs
git for-each-ref --format="%(refname)" refs/original/ | xargs -n 1 git update-ref -d

# Force garbage collection
git gc --prune=now --aggressive

echo "Sensitive data removed from history!"
echo "IMPORTANT: Force push required to update remote repository"
echo "Run: git push origin --force --all"
echo "Run: git push origin --force --tags"