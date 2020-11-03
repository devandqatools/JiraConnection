using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace TFRestApiApp
{
    class AIRTDB
    {
        public static DataTable connectAndGetDataFromAIRTDB()
        {
            string Query = @"SELECT    CASE 
         WHEN gl.grp = 'Core Services Engineering and Operations' THEN 'CSEO'
         WHEN gl.grp = 'WWL' THEN 'WWL'
         WHEN gl.grp = 'OCP' THEN 'OCP'
       END AS[Group],
       CASE
         WHEN sg.subgrp = 'Corporate Functions Engineering' THEN 'CFE'
         WHEN sg.subgrp = 'Core Platform Engineering' THEN 'CPE'
         WHEN sg.subgrp = 'Digital Security and Risk Engineering' THEN 'DSRE'
         WHEN sg.subgrp = 'End User Services Engineering' THEN 'EUSE'
         WHEN sg.subgrp = 'Sales Marketing and Shared Experiences' THEN 'SMSE'
         WHEN sg.subgrp = 'Operations' THEN 'Operations'
         WHEN sg.subgrp = 'Learning' THEN 'Learning'
         WHEN sg.subgrp = 'LeX' THEN 'LeX'
         WHEN sg.subgrp = 'Readiness' THEN 'Readiness'
         WHEN sg.subgrp = 'WW Readiness' THEN 'WW Readiness'
         WHEN sg.subgrp = 'WWL' THEN 'WWL'
         WHEN sg.subgrp = 'OCP' THEN 'OCP'
         WHEN sg.subgrp = 'Operations' THEN 'Operations'
       END AS
       [Organization],
       invappdata.recid, 
       CASE
         WHEN invappdata.componentid IS NOT NULL THEN
         'https://airt.azurewebsites.net/InventoryDetails/'
         + CONVERT(VARCHAR, invappdata.recid) + '/'
         + invappdata.componentid
         WHEN invappdata.componentid IS NULL THEN
         'https://airt.azurewebsites.net/InventoryDetails/'
         + CONVERT(VARCHAR, invappdata.recid) + '/'
         + 'NULL'
       END AS
       [Link to AIRT],
       COALESCE(cseappstdata.namedesc, noncseappstdata.namedesc) AS
       [Digital Property], 
       gddl.grade AS
       InventoryGrade, 
       gradelookups.grade AS
       AssesmentGrade, 
       prioritylookups.priority AS[Priority],
       osl.opsstatus, 
       CASE
         WHEN invassessments.assactivityid IN (5,7)
              AND invassessments.assstatusid = '7' THEN 'Yes'
         ELSE 'No'
       END AS
       AssessmentDone, 
       assessmentactivitylookups.assactivity, 
       assessmentstatuslookups.assstatus, 
       invassessments.datestarted, 
       invassessments.datecompleted, 
       assessmentmethodlookups.assmethod, 
       invassessments.createddt, 
       invassessments.cratedby, 
       DependencyDetails.dependency, 
       Concat_ws(';', dd.coredependency, dd.firstpartydepen, dd.thirdpartydepen,
       dd.otherdependency)                                       AS
       DependencyName,
       vit.tags, 
       cseappstdata.subgrpacclead AS[PM Owner],
       CASE
         WHEN invappdata.acccont IS NULL THEN
         view_defaultaccessibilityowner.accessibilityowner
         ELSE invappdata.acccont
       END                                                       AS[AccLead], 
       cseappstdata.srvoffering, 
       CASE
         WHEN invappdata.recid IS NOT NULL
              AND DependencyDetails.dependency IS NOT NULL THEN
         'https://airt.azurewebsites.net/DependencyTracker/'
         + CONVERT(VARCHAR, invappdata.recid)
       END AS
       [Dependency Record],
       componentsizelookups.size, 
       invappdata.targetcompliance AS
       [TargetComplianceDt]
    FROM[dbo].[invappdata] InvAppData
    LEFT JOIN(SELECT[invassessments].recid,
                      Max([invassessments].assid) AS latestAssesment
                  FROM[dbo].[invassessments]
                  WHERE[invassessments].isdeleted = 0
                         AND[invassessments].assactivityid IN(1,5)
                         AND[invassessments].assstatusid IN(7)
                  GROUP BY[invassessments].recid) latestAssDetails
            ON invappdata.recid = latestAssDetails.recid
       LEFT JOIN[dbo].[invassessments]
        InvAssessments
             ON latestAssDetails.latestassesment = invassessments.assid
      LEFT JOIN[dbo].[assessmentactivitylookups]
        AssessmentActivityLookups
            ON invassessments.assactivityid = 
                 assessmentactivitylookups.assactivityid
       LEFT JOIN[dbo].[assessmentstatuslookups]
        AssessmentStatusLookups
             ON invassessments.assstatusid = 
                 assessmentstatuslookups.assstatusid
       LEFT JOIN[dbo].[assessmentmethodlookups]
        AssessmentMethodLookups
             ON invassessments.assmethodid = 
                 assessmentmethodlookups.assmethodid
       LEFT JOIN[dbo].[gradelookups]
        GradeLookups
             ON invassessments.gradeid = gradelookups.gradeid
      LEFT JOIN[dbo].[cseappstdata]
        CSEAppSTData
            ON invappdata.recid = cseappstdata.recid
     LEFT JOIN[dbo].[noncseappstdata]
        NonCSEAppSTData
           ON invappdata.recid = noncseappstdata.recid
    LEFT JOIN[dbo].[grouplookups]
        gl
          ON invappdata.grpid = gl.grpid
   LEFT JOIN[dbo].[subgroups]
        sg
         ON invappdata.subgrpid = sg.subgrpid
  LEFT JOIN[dbo].[gradelookups]
        gddl
        ON invappdata.gradeid = gddl.gradeid
LEFT JOIN[dbo].[prioritylookups]
        PriorityLookups
       ON invappdata.priid = prioritylookups.priorityid
LEFT JOIN[dbo].[opsstatuslookups]
        osl
      ON invappdata.opsstatusid = osl.opsstatusid
LEFT JOIN[dbo].view_defaultaccessibilityowner
     ON view_defaultaccessibilityowner.recid = invappdata.recid
LEFT JOIN componentsizelookups
              ON componentsizelookups.id = invappdata.componentsizeid
       LEFT JOIN (SELECT ia.recid,
                         CASE
                           WHEN r1.depcount > 0 
                                AND r2.depcount > 0 
                                AND r4.depcount > 0 THEN
                           '1st Party - Product Group,3rd Party - Supplier,Core' 
                           WHEN r1.depcount > 0 
                                AND r2.depcount > 0 THEN
                           '1st Party - Product Group,3rd Party - Supplier' 
                           WHEN r1.depcount > 0 
                                AND r4.depcount > 0 THEN
                           '1st Party - Product Group,Core' 
                           WHEN r2.depcount > 0 
                                AND r4.depcount > 0 THEN
                           '3rd Party - Supplier,Core' 
                           WHEN r1.depcount > 0 THEN 'FirstParty' 
                           WHEN r2.depcount > 0 THEN '3rd Party - Supplier' 
                           WHEN r4.depcount > 0 THEN 'Core' 
                           ELSE NULL
                         END AS Dependency
                  FROM   [dbo].[invappdata] ia
                         LEFT JOIN (SELECT recid,
                                           deptypeid,
                                           Count(*) AS depCount
                                    FROM   [dbo].[invdependencies]
                                    WHERE  isdeleted = 0
                                    GROUP BY recid,
                                              deptypeid) r1
                                ON ia.recid = r1.recid
                                   AND r1.deptypeid = 1 
                         LEFT JOIN(SELECT recid,
                                           deptypeid,
                                           Count(*) AS depCount
                                    FROM   [dbo].[invdependencies]
                                    WHERE  isdeleted = 0
                                    GROUP BY recid,
                                              deptypeid) r2
                                ON ia.recid = r2.recid
                                   AND r2.deptypeid = 2 
                         LEFT JOIN(SELECT recid,
                                           deptypeid,
                                           Count(*) AS depCount
                                    FROM   [dbo].[invdependencies]
                                    WHERE  isdeleted = 0
                                    GROUP BY recid,
                                              deptypeid) r3
                                ON ia.recid = r3.recid
                                   AND r3.deptypeid = 3 
                         LEFT JOIN(SELECT recid,
                                           deptypeid,
                                           Count(*) AS depCount
                                    FROM   [dbo].[invdependencies]
                                    WHERE  isdeleted = 0
                                    GROUP BY recid,
                                              deptypeid) r4
                                ON ia.recid = r4.recid
                                   AND r4.deptypeid = 4) DependencyDetails
              ON invappdata.recid = DependencyDetails.recid
       LEFT JOIN[dbo].[view_inventorytags]
        vit
             ON invappdata.recid = vit.recid
      LEFT JOIN(SELECT d.recid,
                        d1.depen AS firstpartyDepen,
                        d2.depen AS thirdPartyDepen,
                        d3.depen AS otherDependency,
                        d4.depen AS coredependency
                 FROM   (SELECT recid

                         FROM   [dbo].[invdependencies]
                          WHERE  isdeleted = 0

                         GROUP BY recid) d
                       LEFT JOIN(SELECT dDependencyTypeLookups.recid,
                                         dDependencyTypeLookups.deptypeid,
                                         (SELECT dsl.dependency + ';' 

                                          FROM   (SELECT
                                         invdependencies.recid,
            invdependencies.depid,
            dl.dependency,
            invdependencies.deptypeid,
            dependencytypelookups.dependencytype
            FROM   [dbo].[invdependencies]
            InvDependencies
            LEFT JOIN[dbo].[dependencytypelookups]
            DependencyTypeLookups
            ON invdependencies.deptypeid =
            dependencytypelookups.deptypeid
            LEFT JOIN [dbo].[dependencylookups] dl
            ON invdependencies.depid = dl.depid
            WHERE invdependencies.isdeleted = 0) dsl
            WHERE  dsl.deptypeid = 
            dDependencyTypeLookups.deptypeid
            AND dsl.recid = dDependencyTypeLookups.recid
            FOR xml path('')) AS depen
            FROM(SELECT invdependencies.recid,
            invdependencies.depid,
            dl.dependency,
            invdependencies.deptypeid,
            dependencytypelookups.dependencytype
            FROM   [dbo].[invdependencies] InvDependencies
            LEFT JOIN[dbo].[dependencytypelookups]
            DependencyTypeLookups
            ON invdependencies.deptypeid =
            dependencytypelookups.deptypeid
            LEFT JOIN [dbo].[dependencylookups] dl
            ON invdependencies.depid = dl.depid
            WHERE invdependencies.isdeleted = 0)
            dDependencyTypeLookups
            GROUP  BY dDependencyTypeLookups.recid,
            dDependencyTypeLookups.deptypeid) d1
            ON d.recid = d1.recid
            AND d1.deptypeid = 1 
            LEFT JOIN(SELECT dDependencyTypeLookups.recid,
            dDependencyTypeLookups.deptypeid,
            (SELECT dsl.dependency + ';' 
            FROM   (SELECT invdependencies.recid,
            invdependencies.depid,
            dl.dependency,
            invdependencies.deptypeid,
            dependencytypelookups.dependencytype
            FROM   [dbo].[invdependencies]
            InvDependencies
            LEFT JOIN[dbo].[dependencytypelookups]
            DependencyTypeLookups
            ON invdependencies.deptypeid =
            dependencytypelookups.deptypeid
            LEFT JOIN [dbo].[dependencylookups] dl
            ON invdependencies.depid = dl.depid
            WHERE invdependencies.isdeleted = 0) dsl
            WHERE  dsl.deptypeid = 
            dDependencyTypeLookups.deptypeid
            AND dsl.recid = dDependencyTypeLookups.recid
            FOR xml path('')) AS depen
            FROM(SELECT invdependencies.recid,
            invdependencies.depid,
            dl.dependency,
            invdependencies.deptypeid,
            dependencytypelookups.dependencytype
            FROM   [dbo].[invdependencies] InvDependencies
            LEFT JOIN[dbo].[dependencytypelookups]
            DependencyTypeLookups
            ON invdependencies.deptypeid =
            dependencytypelookups.deptypeid
            LEFT JOIN [dbo].[dependencylookups] dl
            ON invdependencies.depid = dl.depid
            WHERE invdependencies.isdeleted = 0)
            dDependencyTypeLookups
            GROUP  BY dDependencyTypeLookups.recid,
            dDependencyTypeLookups.deptypeid) d2
            ON d.recid = d2.recid
            AND d2.deptypeid = 2 
            LEFT JOIN(SELECT dDependencyTypeLookups.recid,
            dDependencyTypeLookups.deptypeid,
            (SELECT dsl.dependency + ';' 
            FROM   (SELECT invdependencies.recid,
            invdependencies.depid,
            dl.dependency,
            invdependencies.deptypeid,
            dependencytypelookups.dependencytype
            FROM   [dbo].[invdependencies]
            InvDependencies
            LEFT JOIN[dbo].[dependencytypelookups]
            DependencyTypeLookups
            ON invdependencies.deptypeid =
            dependencytypelookups.deptypeid
            LEFT JOIN [dbo].[dependencylookups] dl
            ON invdependencies.depid = dl.depid
            WHERE invdependencies.isdeleted = 0) dsl
            WHERE  dsl.deptypeid = 
            dDependencyTypeLookups.deptypeid
            AND dsl.recid = dDependencyTypeLookups.recid
            FOR xml path('')) AS depen
            FROM(SELECT invdependencies.recid,
            invdependencies.depid,
            dl.dependency,
            invdependencies.deptypeid,
            dependencytypelookups.dependencytype
            FROM   [dbo].[invdependencies] InvDependencies
            LEFT JOIN[dbo].[dependencytypelookups]
            DependencyTypeLookups
            ON invdependencies.deptypeid =
            dependencytypelookups.deptypeid
            LEFT JOIN [dbo].[dependencylookups] dl
            ON invdependencies.depid = dl.depid
            WHERE invdependencies.isdeleted = 0)
            dDependencyTypeLookups
            GROUP  BY dDependencyTypeLookups.recid,
            dDependencyTypeLookups.deptypeid) d3
            ON d.recid = d3.recid
            AND d3.deptypeid = 3 
            LEFT JOIN(SELECT dDependencyTypeLookups.recid,
            dDependencyTypeLookups.deptypeid,
            (SELECT dsl.dependency + ';' 
            FROM   (SELECT invdependencies.recid,
            invdependencies.depid,
            dl.dependency,
            invdependencies.deptypeid,
            dependencytypelookups.dependencytype
            FROM   [dbo].[invdependencies]
            InvDependencies
            LEFT JOIN[dbo].[dependencytypelookups]
            DependencyTypeLookups
            ON invdependencies.deptypeid =
            dependencytypelookups.deptypeid
            LEFT JOIN [dbo].[dependencylookups] dl
            ON invdependencies.depid = dl.depid
            WHERE invdependencies.isdeleted = 0) dsl
            WHERE  dsl.deptypeid = 
            dDependencyTypeLookups.deptypeid
            AND dsl.recid = dDependencyTypeLookups.recid
            FOR xml path('')) AS depen
            FROM(SELECT invdependencies.recid,
            invdependencies.depid,
            dl.dependency,
            invdependencies.deptypeid,
            dependencytypelookups.dependencytype
            FROM   [dbo].[invdependencies] InvDependencies
            LEFT JOIN[dbo].[dependencytypelookups]
            DependencyTypeLookups
            ON invdependencies.deptypeid =
            dependencytypelookups.deptypeid
            LEFT JOIN [dbo].[dependencylookups] dl
            ON invdependencies.depid = dl.depid
            WHERE invdependencies.isdeleted = 0)
            dDependencyTypeLookups
            GROUP  BY dDependencyTypeLookups.recid,
            dDependencyTypeLookups.deptypeid) d4
            ON d.recid = d4.recid
            AND d4.deptypeid = 4) dd
            ON invappdata.recid = dd.recid
            WHERE  invappdata.grpid IN( 2 )
                   AND invappdata.subgrpid IN ( 2, 3, 4, 6,
                                                8, 10, 141, 142,
                                                143, 144, 145, 146 )
                   AND invappdata.isdeleted = 0 
                   AND prioritylookups.priorityid IN ( 1,2,3,4 )
                   AND invappdata.opsstatusid != 7 
                   AND (vit.tags != 'non-inventory' 
                          OR vit.tags IS NULL )  
            ORDER BY invappdata.recid ASC";

            DataTable dt = new DataTable();
            string dbConn = null;
            dbConn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
            //dbConn = @"Data Source = airtproddbserver.database.windows.net; user id=AIRTReader; password=Reader_AIRT@12; Initial Catalog = AIRTProd;";
            SqlConnection sqlConnection = new SqlConnection(dbConn);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = Query;
            cmd.Connection = sqlConnection;
            sqlConnection.Open();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dt);
            sqlConnection.Close();
            return dt;

        }
    }
}
