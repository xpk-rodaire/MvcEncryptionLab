USE [Encryption]

SELECT COUNT(*) AS FirstNameCount FROM FirstNames
SELECT COUNT(*) AS LastNameCount FROM LastNames
SELECT COUNT(*) AS ZipCodeCount FROM CaZipCodes

DELETE FROM People
DELETE FROM Addresses
