# DNS Records Documentation - fdx.trading
**Last Updated**: January 21, 2025  
**Provider**: Namecheap  
**Status**: ✅ All Records Verified and Active

---

## 📧 Email Configuration Overview

| Service | Purpose | Status |
|---------|---------|--------|
| **Office 365** | Inbound email (mailboxes) | ✅ Active |
| **SendGrid** | Outbound email (transactional) | ✅ Verified |
| **SPF** | Email authentication | ✅ Configured |
| **DKIM** | Email signing | ✅ Active |
| **DMARC** | Email policy | ✅ Monitoring mode |

---

## 🔧 DNS Records

### MX Records (Mail Exchange)
| Type | Host | Value | Priority | TTL | Purpose |
|------|------|-------|----------|-----|---------|
| MX | @ | fdx-trading.mail.protection.outlook.com | 0 | Automatic | Routes inbound email to Office 365 |

### CNAME Records (SendGrid Authentication)
| Type | Host | Value | TTL | Status | Purpose |
|------|------|-------|-----|--------|---------|
| CNAME | em1370 | sendgrid.net | Automatic | ✅ | SendGrid tracking |
| CNAME | em2675 | u55349844.wl095.sendgrid.net | Automatic | ✅ | SendGrid auth |
| CNAME | em5791 | u55349844.wl095.sendgrid.net | Automatic | ✅ | SendGrid auth |
| CNAME | em7296 | u55349844.wl095.sendgrid.net | Automatic | ✅ Verified | Primary SendGrid domain |
| CNAME | em7520 | sendgrid.net | Automatic | ✅ | SendGrid tracking |
| CNAME | s1._domainkey | s1.domainkey.u55349844.wl095.sendgrid.net | Automatic | ✅ | DKIM key 1 |
| CNAME | s2._domainkey | s2.domainkey.u55349844.wl095.sendgrid.net | Automatic | ✅ | DKIM key 2 |

### TXT Records (Authentication & Policy)
| Type | Host | Value | TTL | Purpose |
|------|------|-------|-----|---------|
| TXT | @ | `v=spf1 include:spf.protection.outlook.com include:sendgrid.net ~all` | Automatic | SPF - Authorizes Office 365 & SendGrid |
| TXT | _dmarc | `v=DMARC1; p=none; rua=mailto:dmarc_agg@vali.email` | Automatic | DMARC - Monitor mode, reports to vali.email |

---

## 📊 Quick Reference Table (CSV Format)

```csv
Type,Host,Value,Priority,TTL,Purpose,Status
MX,@,fdx-trading.mail.protection.outlook.com,0,Automatic,Office 365 inbound mail,Active
CNAME,em1370,sendgrid.net,,Automatic,SendGrid tracking,Active
CNAME,em2675,u55349844.wl095.sendgrid.net,,Automatic,SendGrid auth,Active
CNAME,em5791,u55349844.wl095.sendgrid.net,,Automatic,SendGrid auth,Active
CNAME,em7296,u55349844.wl095.sendgrid.net,,Automatic,Primary SendGrid domain,Verified
CNAME,em7520,sendgrid.net,,Automatic,SendGrid tracking,Active
CNAME,s1._domainkey,s1.domainkey.u55349844.wl095.sendgrid.net,,Automatic,DKIM key 1,Active
CNAME,s2._domainkey,s2.domainkey.u55349844.wl095.sendgrid.net,,Automatic,DKIM key 2,Active
TXT,@,"v=spf1 include:spf.protection.outlook.com include:sendgrid.net ~all",,Automatic,SPF record,Active
TXT,_dmarc,"v=DMARC1; p=none; rua=mailto:dmarc_agg@vali.email",,Automatic,DMARC policy,Monitoring
```

---

## ✅ Verification Checklist

- [x] **MX Record**: Points to Office 365
- [x] **SPF Record**: Includes both Office 365 and SendGrid
- [x] **DKIM Keys**: Both s1 and s2 configured
- [x] **DMARC Record**: Set to monitoring mode
- [x] **SendGrid Domain**: Verified (em7296)
- [x] **SendGrid Sender**: udi@fdx.trading verified
- [x] **Link Branding**: Active for fdx.trading

---

## 📧 Email Flow

```
Inbound Email Flow:
Internet → MX Record → Office 365 → User Mailbox

Outbound Email Flow:
Application → SendGrid API → DKIM Sign → SPF Check → Recipient
```

---

## 🔒 Security Status

| Check | Status | Notes |
|-------|--------|-------|
| SPF | ✅ Pass | Both Office 365 & SendGrid authorized |
| DKIM | ✅ Pass | Keys active and signing |
| DMARC | ⚠️ Monitor | p=none (monitoring only, not enforcing) |
| Reputation | 100% | SendGrid reputation score |

---

## 📝 Notes

1. **DMARC Policy**: Currently in monitoring mode (`p=none`). Consider moving to `p=quarantine` after monitoring reports.
2. **Reports**: DMARC aggregate reports sent to `dmarc_agg@vali.email`
3. **SendGrid**: Using Web API integration (not SMTP relay)
4. **Office 365**: Handles all inbound email and user mailboxes
5. **Transactional Email**: All app-generated emails (magic links, notifications) sent via SendGrid

---

## 🚀 Next Steps

1. **Test Email Delivery**: Send test emails to Gmail, Outlook, Yahoo
2. **Check Headers**: Verify SPF/DKIM/DMARC all show PASS
3. **Monitor DMARC Reports**: Review reports at vali.email
4. **Consider DMARC Enforcement**: After successful monitoring, update to `p=quarantine`

---

## 📞 Support Contacts

- **Namecheap Support**: DNS record changes
- **SendGrid Support**: Email delivery issues
- **Office 365 Admin**: Mailbox and MX issues

---

**Document Version**: 1.0  
**Created By**: FDX Trading IT Team  
**Platform**: FoodX B2B Trading Platform