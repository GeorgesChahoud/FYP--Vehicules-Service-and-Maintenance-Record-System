-- ==============================================
-- Diagnostic Query for Receipt Issues
-- ==============================================
-- Run this to see what's in your database

USE [CarHubDB];
GO

-- 1. Check all receipts and their totals
SELECT 
    r.ID as ReceiptID,
    r.AppointmentID,
    r.DateANDTime,
    r.Total as ReceiptTotal,
    a.EmployeeID,
    e.FeeByService as EmployeeFee
FROM Receipts r
INNER JOIN Appointments a ON r.AppointmentID = a.ID
INNER JOIN Employees e ON a.EmployeeID = e.ID
ORDER BY r.ID DESC;

PRINT '';
PRINT '--- RECEIPT PARTS DATA ---';
PRINT '';

-- 2. Check receipt parts details
SELECT 
    rp.ID as ReceiptPartID,
    rp.ReceiptID,
    rp.PartID,
    p.PartName,
    rp.QuantityUsed,
    rp.PriceAtTime,
    (rp.QuantityUsed * rp.PriceAtTime) as LineTotal
FROM ReceiptParts rp
INNER JOIN Parts p ON rp.PartID = p.ID
ORDER BY rp.ReceiptID, rp.ID;

PRINT '';
PRINT '--- RECEIPT TOTALS BREAKDOWN ---';
PRINT '';

-- 3. Compare receipts with what they SHOULD be
SELECT 
    r.ID as ReceiptID,
    r.Total as CurrentTotal,
    e.FeeByService as ServiceFee,
    ISNULL(SUM(rp.QuantityUsed * rp.PriceAtTime), 0) as PartsCost,
    (e.FeeByService + ISNULL(SUM(rp.QuantityUsed * rp.PriceAtTime), 0)) as CalculatedTotal,
    CASE 
        WHEN r.Total = 0 THEN '? Total is ZERO!'
        WHEN r.Total != (e.FeeByService + ISNULL(SUM(rp.QuantityUsed * rp.PriceAtTime), 0)) THEN '?? Total MISMATCH!'
        ELSE '? Correct'
    END as Status
FROM Receipts r
INNER JOIN Appointments a ON r.AppointmentID = a.ID
INNER JOIN Employees e ON a.EmployeeID = e.ID
LEFT JOIN ReceiptParts rp ON r.ID = rp.ReceiptID
GROUP BY r.ID, r.Total, e.FeeByService
ORDER BY r.ID DESC;

PRINT '';
PRINT '--- ISSUE DETECTION ---';
PRINT '';

-- 4. Find receipts with issues
IF EXISTS (SELECT 1 FROM Receipts WHERE Total = 0)
BEGIN
    PRINT '? FOUND RECEIPTS WITH $0.00 TOTAL!';
    PRINT 'These receipts need to be recalculated.';
    PRINT '';
    
    SELECT 
        r.ID as ReceiptID,
        'Receipt #' + CAST(r.ID AS VARCHAR) + ' has $0 total but should be $' + 
        CAST((e.FeeByService + ISNULL(SUM(rp.QuantityUsed * rp.PriceAtTime), 0)) AS VARCHAR(10)) as Issue
    FROM Receipts r
    INNER JOIN Appointments a ON r.AppointmentID = a.ID
    INNER JOIN Employees e ON a.EmployeeID = e.ID
    LEFT JOIN ReceiptParts rp ON r.ID = rp.ReceiptID
    WHERE r.Total = 0
    GROUP BY r.ID, e.FeeByService;
END
ELSE
BEGIN
    PRINT '? No receipts with $0.00 total found.';
END

PRINT '';
PRINT '--- RECOMMENDATIONS ---';
PRINT '';
PRINT 'If receipts have $0 total:';
PRINT '1. They were created before ReceiptParts table existed';
PRINT '2. Run the FIX script to recalculate totals';
PRINT '';
PRINT 'If receipts have wrong totals:';
PRINT '1. They might have been created with an error';
PRINT '2. Run the FIX script to recalculate';

GO
