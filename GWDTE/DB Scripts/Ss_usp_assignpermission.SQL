-- parse email and assign permission 
IF OBJECT_ID('Ss_usp_assignpermission') IS NOT NULL
DROP PROC Ss_usp_assignpermission
END
GO
CREATE PROC [dbo].[Ss_usp_assignpermission] @fromuser  VARCHAR(100), 
                                           @cclist    VARCHAR(100), 
                                           @perm_str  VARCHAR(4000)='', 
                                           @delimiter CHAR(1)=',' 
AS 
  BEGIN 
      -- validate cclist requesting permission.   
      -- split @cclist    
      SET nocount ON 
      DECLARE @ccxml XML 

      SELECT @ccxml = CONVERT(XML, '<root><s>' 
                                   + Replace(@cclist, @delimiter, '</s><s>') 
                                   + '</s></root>') 

      -- validate the cc list with person in the cc list table.
/*
      IF NOT EXISTS (SELECT 1 
                     FROM   @ccxml.nodes('/root/s') T(cclist) 
      --JOIN ss_tbl_cc_List ON tcl_cc_list=T.cclist.value('.','varchar(100)')   
                     WHERE  t.cclist.value('.', 'varchar(100)') IN 
                            ( 'Ahmad Osama' )) 
        BEGIN 
            SELECT 'cclist is either empty or not matching' 

            RETURN 0; 
        END 
*/
      -- Parse email subject   
      DECLARE @Emailxml  XML, 
              @ToUser    VARCHAR(500), 
              @Srvr      VARCHAR(100), 
              @perm_name VARCHAR(20), 
              @Count     INT 
      DECLARE @database   VARCHAR(500), 
              @perm_count INT, 
              @StrSQL     VARCHAR(max) 
      DECLARE @tmp_Rqst TABLE 
        ( 
           sno INT IDENTITY, 
           val VARCHAR(4000) 
        ) 

      SELECT @Emailxml = CONVERT(XML, '<root><s>' 
                                      + Replace(@perm_str, @delimiter, '</s><s>' 
                                      ) 
                                      + '</s></root>') 

      INSERT INTO @tmp_Rqst 
      SELECT t.c.value('.', 'varchar(4000)') 
      FROM   @Emailxml.nodes('/root/s') T(coffee) 

      SET @Srvr=(SELECT Ltrim(Rtrim(val)) 
                 FROM   @tmp_Rqst 
                 WHERE  sno = 2) 
      SET @database=(SELECT Ltrim(Rtrim(val)) 
                     FROM   @tmp_Rqst 
                     WHERE  sno = 3) 
      SET @perm_count=(SELECT Count(*) 
                       FROM   @tmp_Rqst 
                       WHERE  sno > 3) 

      -- verify database  
      IF NOT EXISTS (SELECT 1 
                     FROM   sys.databases 
                     WHERE  [name] = @database) 
        BEGIN 
            SELECT 'The database ' + @database 
                   + ' doesn''t exist at server ' + @Srvr 

            RETURN 0; 
        END 

      SET @ToUser=(SELECT Ltrim(Rtrim(val)) 
                   FROM   @tmp_Rqst 
                   WHERE  sno = 1) 

      -- Verify Login    
      IF NOT EXISTS (SELECT 1 
                     FROM   master..syslogins 
                     WHERE  name = @ToUser) 
        BEGIN 
            SELECT 
'Login with a specified user name doesn''t exists.Please Contact Database Administrator' 

    RETURN 0; 
END 

    SET @Count=4 

    WHILE( @perm_count > 0 ) 
      BEGIN 
          SELECT @perm_name = Rtrim(Ltrim(val)) 
          FROM   @tmp_Rqst 
          WHERE  sno = @count 

          SET @StrSQL='Use ' + @Database 
                      + ' IF EXISTS(select 1    from sysusers u inner join master..syslogins l    on u.sid=l.sid where u.name=''' 
                      + @ToUser + ''' and l.name=''' + @ToUser 
                      + ''') BEGIN EXEC sp_addrolemember ''' + @perm_name 
                      + ''',''' + @ToUser + ''' END ELSE BEGIN CREATE USER [' + @ToUser 
                      + '] FOR LOGIN [' + @ToUser 
                      + '] EXEC sp_addrolemember ''' + @perm_name 
                      + ''',''' + @ToUser + ''' END' 
          SET @Count=@Count + 1 
          SET @perm_count=@perm_count - 1 

          EXECUTE (@StrSQL) 
      END 

    SELECT 'Permission Granted. Please verify' 

    SET nocount OFF 
END