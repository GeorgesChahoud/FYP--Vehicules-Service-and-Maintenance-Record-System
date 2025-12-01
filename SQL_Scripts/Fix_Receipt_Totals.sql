-- ==============================================
-- FIX SCRIPT: Recalculate Receipt Totals
-- ==============================================
-- This will fix receipts that have $0 or incorrect totals
-- Run this AFTER creating the ReceiptParts table

USE [CarHubDB];
GO

PRINT '?? Starting Receipt Total Recalculation...';
PRINT '';

-- Update all receipts with correct totals
UPDATE r
SET r.Total = (
    -- Service fee
    e.FeeByService + 
    -- Plus parts cost
    ISNULL((
        SELECT SUM(rp.QuantityUsed * rp.PriceAtTime)
        FROM ReceiptParts rp
        WHERE rp.ReceiptID = r.ID
    ), 0)
)
FROM Receipts r
INNER JOIN Appointments a ON r.AppointmentID = a.ID
INNER JOIN Employees e ON a.EmployeeID = e.ID;

PRINT '? Receipt totals updated!';
PRINT '';

-- Show the results
PRINT '--- UPDATED RECEIPTS ---';
SELECT 
    r.ID as ReceiptID,
    r.AppointmentID,
    e.FeeByService as ServiceFee,
    ISNULL((
        SELECT SUM(rp.QuantityUsed * rp.PriceAtTime)
        FROM ReceiptParts rp
        WHERE rp.ReceiptID = r.ID
    ), 0) as PartsCost,
    r.Total as NewTotal,
    CASE 
        WHEN r.Total = 0 THEN '? Still ZERO'
        ELSE '? Fixed'
    END as Status
FROM Receipts r
INNER JOIN Appointments a ON r.AppointmentID = a.ID
INNER JOIN Employees e ON a.EmployeeID = e.ID
ORDER BY r.ID DESC;

PRINT '';
PRINT '? Done! All receipt totals have been recalculated.';
PRINT 'Refresh your application to see the changes.';

GO
