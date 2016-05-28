
DECLARE @ValueToFind AS VARCHAR(10) = '564819307'
DECLARE @ValueToFindBinary AS VARBINARY(10) = CONVERT(VARBINARY(10), @ValueToFind)

DECLARE @Id AS int
DECLARE @HashBase64 AS NVARCHAR(64)
DECLARE @SaltBase64 AS NVARCHAR(64)

DECLARE @HashBase64Result AS NVARCHAR(64)

DECLARE @HashBinary AS VARBINARY(MAX)
DECLARE @SaltBinary AS VARBINARY(MAX)

DECLARE @lb_Exit  bit
SET @lb_Exit  = 0

DECLARE db_cursor CURSOR FOR
	SELECT
		f1095c.Form1095CUpstreamDetailTypeId,
		per.PersonFirstNm,
		PER.PersonMiddleNm,
		emp.SSNHash,
		emp.SSNSalt
	FROM
		TY2015_06.Form1095CUpstreamDetailType AS f1095c
		JOIN TY2015_06.EmployeeInformationGrpType AS emp
			ON f1095c.EmployeeInfoGrp_EmployeeInformationGrpTypeId = emp.EmployeeInformationGrpTypeId
		JOIN TY2015_06.OtherCompletePersonNameType AS per
			ON emp.OtherCompletePersonName_OtherCompletePersonNameTypeId = per.OtherCompletePersonNameTypeId
	WHERE
		per.PersonFirstNm = 'STEPHEN'
		AND (per.PersonMiddleNm = 'L' OR per.PersonMiddleNm IS NULL)
	ORDER BY
		f1095c.Form1095CUpstreamDetailTypeId DESC

OPEN db_cursor   
FETCH NEXT FROM db_cursor INTO @Id, @HashBase64, @SaltBase64

WHILE (@@FETCH_STATUS = 0) AND (@lb_Exit = 0)
BEGIN
	SET @SaltBinary = CAST(N'' as xml).value('xs:base64Binary(sql:variable("@SaltBase64"))', 'VARBINARY(MAX)')

	SET @HashBinary = HASHBYTES('SHA2_256', @ValueToFindBinary + @SaltBinary)

	SET @HashBase64Result = CAST(N'' AS XML).value('xs:base64Binary(xs:hexBinary(sql:variable("@HashBinary")))', 'VARCHAR(64)')

	IF @HashBase64 = @HashBase64Result
	BEGIN
		SET @lb_Exit  = 1

		SELECT
			@ValueToFindBinary AS ValueToFindBinary,
			@SaltBase64 AS SaltBase64,
			@HashBase64 AS HashBase64,
			@HashBinary AS HashBinaryResult,
			@HashBase64Result AS HashBase64Result,
			CASE
				WHEN @HashBase64 = @HashBase64Result THEN 'Match!'
				ELSE ''
			END AS Result
	END

    FETCH NEXT FROM db_cursor INTO @Id, @HashBase64, @SaltBase64
END   

CLOSE db_cursor   
DEALLOCATE db_cursor