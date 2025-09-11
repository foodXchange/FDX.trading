-- Add missing columns to Orders table only
-- This script is safe and idempotent

-- Add AutoConfirmEnabled column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'AutoConfirmEnabled')
BEGIN
    ALTER TABLE Orders ADD AutoConfirmEnabled bit NOT NULL DEFAULT(0);
    PRINT 'Added AutoConfirmEnabled column to Orders table';
END
ELSE
BEGIN
    PRINT 'AutoConfirmEnabled column already exists';
END

-- Add CancellationReason column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CancellationReason')
BEGIN
    ALTER TABLE Orders ADD CancellationReason nvarchar(500) NULL;
    PRINT 'Added CancellationReason column to Orders table';
END
ELSE
BEGIN
    PRINT 'CancellationReason column already exists';
END

-- Add CancelledAt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CancelledAt')
BEGIN
    ALTER TABLE Orders ADD CancelledAt datetime2 NULL;
    PRINT 'Added CancelledAt column to Orders table';
END
ELSE
BEGIN
    PRINT 'CancelledAt column already exists';
END

-- Add CommissionAmount column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CommissionAmount')
BEGIN
    ALTER TABLE Orders ADD CommissionAmount decimal(18,2) NOT NULL DEFAULT(0);
    PRINT 'Added CommissionAmount column to Orders table';
END
ELSE
BEGIN
    PRINT 'CommissionAmount column already exists';
END

-- Add ConfirmedAt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'ConfirmedAt')
BEGIN
    ALTER TABLE Orders ADD ConfirmedAt datetime2 NULL;
    PRINT 'Added ConfirmedAt column to Orders table';
END
ELSE
BEGIN
    PRINT 'ConfirmedAt column already exists';
END

-- Add IsDeleted column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    ALTER TABLE Orders ADD IsDeleted bit NOT NULL DEFAULT(0);
    PRINT 'Added IsDeleted column to Orders table';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists';
END

-- Add IsRecurring column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'IsRecurring')
BEGIN
    ALTER TABLE Orders ADD IsRecurring bit NOT NULL DEFAULT(0);
    PRINT 'Added IsRecurring column to Orders table';
END
ELSE
BEGIN
    PRINT 'IsRecurring column already exists';
END

-- Add RecurringOrderTemplateId column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'RecurringOrderTemplateId')
BEGIN
    ALTER TABLE Orders ADD RecurringOrderTemplateId int NULL;
    PRINT 'Added RecurringOrderTemplateId column to Orders table';
END
ELSE
BEGIN
    PRINT 'RecurringOrderTemplateId column already exists';
END

-- Add RecurringSequence column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'RecurringSequence')
BEGIN
    ALTER TABLE Orders ADD RecurringSequence nvarchar(50) NULL;
    PRINT 'Added RecurringSequence column to Orders table';
END
ELSE
BEGIN
    PRINT 'RecurringSequence column already exists';
END

-- Add SubTotal column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'SubTotal')
BEGIN
    ALTER TABLE Orders ADD SubTotal decimal(18,2) NOT NULL DEFAULT(0);
    PRINT 'Added SubTotal column to Orders table';
END
ELSE
BEGIN
    PRINT 'SubTotal column already exists';
END

PRINT 'Orders table column fix completed successfully!';