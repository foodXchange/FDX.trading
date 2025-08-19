# Session Documentation Rules & Template

## Documentation Standards

### 1. Session Header Requirements
Every Claude Code session MUST begin with:
- **Date**: Full date (YYYY-MM-DD)
- **Time**: Start and end time (HH:MM UTC/Local)
- **Session ID**: Unique identifier or sequential number
- **Developer**: Who initiated the session
- **Claude Model**: Model version used

### 2. Mandatory Sections
Each session document MUST include ALL sections below, even if no changes:
- Database Updates
- Code Changes
- Configuration Changes
- Documentation Updates
- Issues & Resolutions
- Next Steps

### 3. File Naming Convention
```
Sessions/YYYY-MM-DD-session-[number]-[brief-description].md
```
Example: `2025-01-18-session-001-initial-setup.md`

---

## Session Documentation Template

```markdown
# Claude Code Session Report

## Session Information
- **Date**: YYYY-MM-DD
- **Start Time**: HH:MM (Timezone)
- **End Time**: HH:MM (Timezone)
- **Duration**: X hours Y minutes
- **Developer**: [Name]
- **Claude Model**: [Model Version]
- **Session Focus**: [Brief description of main objectives]

## Executive Summary
[2-3 sentences summarizing what was accomplished in this session]

## Database Updates

### Schema Changes
- [ ] No schema changes this session
- [ ] Schema changes implemented:
  - [List each change]

### Current Database Structure
**Database**: fdxdb
**Server**: fdx-sql-prod.database.windows.net
**Tables**: 5 total

| Table | Columns | Rows | Relationships | Changes |
|-------|---------|------|---------------|---------|
| Users | 11 | X | → UserEmployments, UserPhones | [None/List] |
| Companies | 16 | X | → UserEmployments | [None/List] |
| UserEmployments | 8 | X | ← Users, Companies | [None/List] |
| UserPhones | 4 | X | ← Users | [None/List] |
| sysdiagrams | X | 0 | System table | [None/List] |

### Data Modifications
- Records Added: [Number and tables]
- Records Updated: [Number and tables]
- Records Deleted: [Number and tables]

### SQL Scripts Executed
```sql
-- List any SQL scripts run
```

## Code Changes

### Files Created
| File Path | Purpose | Lines of Code |
|-----------|---------|---------------|
| [path] | [description] | [LOC] |

### Files Modified
| File Path | Changes Made | Reason |
|-----------|-------------|--------|
| [path] | [what changed] | [why] |

### Files Deleted
| File Path | Reason for Deletion |
|-----------|-------------------|
| [path] | [reason] |

### Code Statistics
- Total Files Created: X
- Total Files Modified: X
- Total Files Deleted: X
- Lines Added: X
- Lines Removed: X

## Configuration Changes

### Application Settings
- [ ] No configuration changes
- [ ] Changes made:
  - [List each change]

### Azure Resources
- [ ] No Azure changes
- [ ] Changes made:
  - [List each change]

### Dependencies
- [ ] No dependency changes
- [ ] Packages added/updated:
  - [List packages with versions]

## Documentation Updates

### Documentation Created
- [List new documentation files]

### Documentation Updated
- [List updated documentation files]

### Key Information Documented
- [Bullet points of important information captured]

## Features Implemented

### Completed Features
- [x] [Feature name and description]

### Partially Completed
- [ ] [Feature name - % complete - what remains]

### UI/UX Changes
- [List any interface changes]

## Issues & Resolutions

### Issues Encountered
1. **Issue**: [Description]
   - **Cause**: [Root cause]
   - **Resolution**: [How it was fixed]
   - **Time Spent**: [Duration]

### Unresolved Issues
1. **Issue**: [Description]
   - **Impact**: [How it affects the project]
   - **Proposed Solution**: [Next steps]

## Testing

### Tests Run
- [ ] No tests run this session
- [ ] Tests executed:
  - [List test types and results]

### Test Coverage
- Current Coverage: X%
- Change from Previous: +/- X%

## Performance Metrics

### Application Performance
- Build Time: X seconds
- Startup Time: X seconds
- Page Load Times: [List key pages]

### Database Performance
- Connection Time: X ms
- Query Performance: [Notable queries]

## Security Updates

### Security Changes
- [ ] No security changes
- [ ] Security improvements:
  - [List changes]

### Credentials Changed
- [ ] No credential changes
- [ ] Updated credentials:
  - [List what was changed - NOT the actual credentials]

## Session Outcomes

### Achievements
✅ [List completed objectives]

### Pending Items
⏳ [List incomplete objectives]

### Blockers
🚫 [List any blocking issues]

## Next Steps

### Immediate (Next Session)
1. [Priority 1 task]
2. [Priority 2 task]
3. [Priority 3 task]

### Short-term (This Week)
- [List tasks]

### Long-term (This Sprint/Month)
- [List tasks]

## Notes & Observations

### Technical Notes
[Any technical observations or learnings]

### Business Notes
[Any business-related observations]

### Recommendations
[Suggestions for improvement]

## Session Metrics

### Productivity Metrics
- Tasks Planned: X
- Tasks Completed: X
- Completion Rate: X%
- Blockers Encountered: X
- Blockers Resolved: X

### Time Distribution
- Database Work: X%
- Coding: X%
- Documentation: X%
- Debugging: X%
- Testing: X%
- Other: X%

## Commands & Queries Used

### Useful Commands
```bash
# List helpful commands used this session
```

### Useful Queries
```sql
-- List helpful SQL queries used
```

## Environment Information
- OS: Windows/Linux/Mac
- .NET SDK: Version
- Azure CLI: Version
- SQL Tools: Version
- Git Status: [Branch, commits]

---

**Session Signed Off By**: [Developer Name]
**Review Status**: [ ] Pending [ ] Reviewed
**Reviewer**: [Name if reviewed]
```

---

## Documentation Rules

### 1. Timing Rules
- Document at START of session (setup)
- Update DURING session (major changes)
- Finalize at END of session (complete summary)

### 2. Detail Level Rules

#### ALWAYS Document:
- Database schema changes (even if none)
- Table relationship changes (even if none)
- Row counts for each table
- Files created/modified/deleted
- Configuration changes
- Credentials changes (not values)
- Errors and resolutions

#### NEVER Skip:
- Session timestamp
- Database current state
- Summary of changes
- Next steps

### 3. Database Documentation Rules

#### For EVERY Session:
```markdown
Current State:
- Tables: [count]
- Relationships: [list all]
- Total Records: [sum]

Changes:
- Schema: [None OR detailed list]
- Data: [None OR detailed list]
- Indexes: [None OR detailed list]
```

### 4. Code Change Rules

#### Format:
```markdown
FILE: [path]
CHANGE TYPE: [Created|Modified|Deleted]
LINES CHANGED: +X -Y
PURPOSE: [why this change]
IMPACT: [what it affects]
```

### 5. Summary Rules

#### Executive Summary Must Include:
1. Primary objective achieved? [Yes/No/Partial]
2. Database modified? [Yes/No]
3. Features added? [List or None]
4. Issues resolved? [Count]
5. Ready for next phase? [Yes/No + reason]

### 6. Continuity Rules

#### Each Session Must Reference:
- Previous session number/date
- Carried over tasks
- Completed carried tasks
- New tasks discovered

### 7. Quality Checklist

Before closing session, verify:
- [ ] All timestamps recorded
- [ ] Database state documented
- [ ] All changes listed
- [ ] Issues documented with resolutions
- [ ] Next steps clearly defined
- [ ] Session summary complete
- [ ] File saved in correct location

---

## Quick Reference Tables

### Session Types
| Type | Code | Focus |
|------|------|-------|
| Feature Development | FD | New features |
| Bug Fix | BF | Fixing issues |
| Database Work | DB | Schema/data changes |
| Documentation | DC | Documentation only |
| Configuration | CF | Settings/config |
| Testing | TS | Testing/QA |
| Deployment | DP | Deployment tasks |

### Priority Levels
| Level | Symbol | Meaning |
|-------|--------|---------|
| Critical | 🔴 | Must fix immediately |
| High | 🟠 | Fix this session |
| Medium | 🟡 | Fix this week |
| Low | 🟢 | Fix when possible |

### Status Indicators
| Status | Symbol | Meaning |
|--------|--------|---------|
| Completed | ✅ | Done |
| In Progress | 🔄 | Working on it |
| Blocked | 🚫 | Cannot proceed |
| Pending | ⏳ | Waiting to start |
| Cancelled | ❌ | Won't do |

---

## Automation Support

### Generate Summary Command
```python
# Script to generate session summary from git logs
git log --since="[session-start]" --until="[session-end]" --pretty=format:"%h - %an, %ar : %s"
```

### Database State Query
```sql
-- Run at start and end of each session
SELECT 
    t.TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) as Columns,
    (SELECT SUM(p.rows) FROM sys.partitions p WHERE p.object_id = OBJECT_ID(t.TABLE_NAME) AND p.index_id IN (0,1)) as Rows
FROM INFORMATION_SCHEMA.TABLES t
WHERE t.TABLE_TYPE = 'BASE TABLE'
ORDER BY t.TABLE_NAME;
```

---

**Document Version**: 1.0.0
**Last Updated**: 2025-01-18
**Next Review**: 2025-02-01