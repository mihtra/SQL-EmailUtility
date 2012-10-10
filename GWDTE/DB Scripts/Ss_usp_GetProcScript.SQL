set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
GO
ALTER proc [dbo].[ss_usp_GetProcScript]
AS
BEGIN 

CREATE TABLE #tmp(proc_text varchar(max))
insert into #tmp
EXEC('sp_helptext ss_usp_AssignPermission')

truncate table ss_tbl_ProcText

INSERT INTO ss_tbl_ProcText
SELECT 'IF OBJECT_ID(''ss_usp_AssignPermission'') IS NOT NULL ' 
INSERT INTO ss_tbl_ProcText
SELECT 'BEGIN' 
INSERT INTO ss_tbl_ProcText
SELECT 'DROP PROC ss_usp_AssignPermission' 
INSERT INTO ss_tbl_ProcText
SELECT 'END '
INSERT INTO ss_tbl_ProcText
SELECT 'GO'
INSERT INTO ss_tbl_ProcText
SELECT proc_Text from #tmp

SELECT * FROM ss_tbl_ProcText
END
SET ansi_nulls ON 
SET quoted_identifier ON 