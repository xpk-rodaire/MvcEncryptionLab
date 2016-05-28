DECLARE @SsnToFindBinary AS varbinary(10) = convert(varbinary(10), '000005001')

DECLARE @SaltBase64 AS nvarchar(64) = 'MVCpjGVU/wRUBLWeBL6AYn8r6onWrQVQ/3VzKYgTF14='
DECLARE @SaltBinary AS varbinary(max) = cast(N'' as xml).value('xs:base64Binary(sql:variable("@SaltBase64"))', 'varbinary(max)')

DECLARE @HashBinary VARBINARY(MAX)
DECLARE @HashBase64 VARCHAR(64)

SET @HashBinary = HASHBYTES('SHA2_256', @SsnToFindBinary + @SaltBinary)  -- this returns varbinary (maximum 8000 bytes)

SET @HashBase64 = CAST(N'' AS XML).value('xs:base64Binary(xs:hexBinary(sql:variable("@HashBinary")))', 'VARCHAR(64)')

SELECT
	@SsnToFindBinary AS SsnToFindBinary,
	@SaltBinary AS SsnSaltBinary,
	@HashBinary AS HashBinary,
	@HashBase64 AS HashBase64