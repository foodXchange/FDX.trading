# Price Negotiation System - Testing Checklist

## 🚀 Pre-Testing Setup

### 1. Database Setup
- [ ] Run the SQL migration script: `FoodX.Admin\Scripts\AddNegotiationTables.sql`
- [ ] Verify tables created: `NegotiationSessions` and `NegotiationMessages`
- [ ] Check database connection in app settings

### 2. Application Configuration
- [ ] Verify SignalR is configured in Program.cs
- [ ] Check SendGrid API key for email notifications (optional)
- [ ] Ensure authentication is working

### 3. Start Application
```powershell
cd C:\Users\fdxadmin\source\repos\FDX.trading\FoodX.Admin
dotnet run --urls http://localhost:5195
```

## 📋 Testing Scenarios

### Scenario 1: Smart Supplier Matching
**As a Buyer:**
1. [ ] Navigate to Create RFQ page
2. [ ] Fill in RFQ details with multiple product lines
3. [ ] Submit the RFQ
4. [ ] Verify smart matching algorithm suggests relevant suppliers
5. [ ] Check that suppliers are ranked by match score
6. [ ] Confirm email notifications sent to matched suppliers

**Expected Results:**
- Suppliers sorted by match percentage
- Match factors visible (product match, location, performance)
- Top 10 suppliers automatically notified

### Scenario 2: Starting a Negotiation
**As a Buyer:**
1. [ ] Go to RFQ list page
2. [ ] Select an RFQ with received quotes
3. [ ] Click "Negotiate" on a supplier quote
4. [ ] Verify negotiation session is created
5. [ ] Check redirect to negotiation chat page

**Expected Results:**
- New negotiation session created
- Initial price displayed
- Chat interface loaded

### Scenario 3: Real-Time Chat Negotiation
**As a Buyer:**
1. [ ] Navigate to `/portal/negotiation/{sessionId}`
2. [ ] Send a text message
3. [ ] Send a counter-offer with new price
4. [ ] Use quick counter-offer buttons (-5%, -10%, etc.)
5. [ ] Test typing indicator
6. [ ] Accept or reject an offer

**As a Supplier (separate browser/incognito):**
1. [ ] Navigate to same negotiation session
2. [ ] Verify real-time message receipt
3. [ ] See typing indicator when buyer types
4. [ ] Send counter-offer
5. [ ] Accept final offer

**Expected Results:**
- Messages appear instantly on both sides
- Typing indicators work
- Price proposals highlighted
- Session status updates when accepted/rejected

### Scenario 4: Negotiations Dashboard
**As a Buyer:**
1. [ ] Navigate to `/portal/negotiations`
2. [ ] View statistics (Active, Accepted, Rejected, Avg Savings)
3. [ ] Check Active Negotiations tab
4. [ ] Check Completed tab
5. [ ] Check Rejected tab
6. [ ] Click to continue an active negotiation

**Expected Results:**
- All negotiations listed correctly
- Statistics calculated properly
- Tabs filter negotiations by status
- Can resume active negotiations

### Scenario 5: Notification System
**Test Points:**
1. [ ] New RFQ triggers supplier notifications
2. [ ] Quote submission triggers buyer notification
3. [ ] Negotiation messages trigger notifications
4. [ ] In-app notifications appear in UI
5. [ ] Email notifications sent (if configured)

**Expected Results:**
- Notifications created for key events
- Email templates render correctly
- In-app notifications are clickable

## 🔍 Edge Cases to Test

### Performance Testing
- [ ] Create negotiation with 100+ messages
- [ ] Test with multiple concurrent negotiations
- [ ] Verify caching works for smart matching
- [ ] Check database query performance

### Error Handling
- [ ] Try to negotiate without being logged in
- [ ] Send empty messages
- [ ] Enter invalid price values
- [ ] Disconnect and reconnect SignalR
- [ ] Test with network interruptions

### Security Testing
- [ ] Verify users can only see their negotiations
- [ ] Check role-based access (Buyer vs Supplier)
- [ ] Test SQL injection in chat messages
- [ ] Verify price history is immutable

## 📊 Test Results Summary

| Component | Status | Notes |
|-----------|--------|-------|
| Smart Matching | ⏳ | |
| Negotiation Creation | ⏳ | |
| Real-Time Chat | ⏳ | |
| Price Proposals | ⏳ | |
| Notifications | ⏳ | |
| Dashboard | ⏳ | |
| Security | ⏳ | |
| Performance | ⏳ | |

## 🐛 Known Issues
1. Build errors in IdentityComponentsEndpointRouteBuilderExtensions.cs (non-blocking)
2. Database migration needs manual SQL execution
3. SendGrid configuration required for email notifications

## ✅ Sign-off Checklist
- [ ] All core features working
- [ ] No critical bugs found
- [ ] Performance acceptable
- [ ] Security verified
- [ ] Ready for UAT

## 📝 Notes
- SignalR hub endpoint: `/hubs/negotiation`
- Main UI routes:
  - `/portal/negotiations` - List view
  - `/portal/negotiation/{id}` - Chat view
  - `/portal/buyer/negotiations` - Buyer dashboard
  - `/portal/supplier/negotiations` - Supplier dashboard

## 🚦 Next Steps
1. Apply database migration to production
2. Configure SendGrid for production emails
3. Set up monitoring for SignalR connections
4. Plan load testing for concurrent negotiations
5. Create user training materials

---
**Test Date:** _________________
**Tested By:** _________________
**Environment:** Development / Staging / Production