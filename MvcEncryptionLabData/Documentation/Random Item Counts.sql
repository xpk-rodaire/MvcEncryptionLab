USE [Encryption]

SELECT COUNT(*) AS FirstNameCount FROM FirstNames

SELECT
	MAX(CumulativeFrequency) AS FirstNameMaxCumFreq,
	IsMale
FROM
	FirstNames
GROUP BY
	IsMale

SELECT COUNT(*) AS LastNameCount FROM LastNames
SELECT MAX(CumulativeFrequency) AS LastNameMaxCumFreq FROM LastNames

SELECT COUNT(*) AS ZipCodeCount FROM CaZipCodes

SELECT RAND() * 100 * 0.90483
--SELECT RAND()