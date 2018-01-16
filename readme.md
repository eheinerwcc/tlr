# TLR - Time and Leave Reporting Application

## Overview

The Time and Leave Reporting application is used by multiple institutions for employees to report time and leave.

## Version 1.4 changes - I-1433

The changes in this version center around providing functionality for hourly employees to report leave and have that leave flow to the export files generated and uploaded by payroll staff. The main changes are to the part-time timesheet (the one provided to hourly employees), supervisor employee leave balance view, and the payroll time file export and the multiple database stored procedures that support them.

- Part-time timesheet - Update to allow an eligible hourly employee to add leave and show a small leave balance grid (similar to exempt and classified timesheet). Because hourly leave cannot be assigned to an FWS or FWO budget there is also now a warning for supervisors as such when they are applying hours to budgets during the timesheet approval process. Note: There is currently no built-in stop (see: time constraints) for the case that they ignore the warning. You will need to have an audit process/report to find and fix this. Your payroll will likely want to check that any employees with FWS or FWS budget type have an alternate budget to apply hourly leave hours to.
- The leave balance page is now linked for eligible hourly employees in the left sidebar
- Supervisors will now see eligible hourly employee balances listed on their Employees' Balances page.
- The TIME file export has been updated to generate both a time file _and_ a leave file and to show both in the files list.

Because we started this work when we had no specific direction from the state board, we had to make a number of assumptions about how hourly leave would operate and be accounted for in the HP. We still have little specific direction from the state board, but a couple assumptions have proven false.  Noted below are some assumption and whether this distribution accounts for said proven-false assumptions - in some cases, due to time constraints in distributing this, fixes for false assumption ARE NOT included, and you will be responsible for these.
 
#### Assumptions

Please check back here, as assumptions will be added/updated here as more information is received from the state board.

- Eligible hourly students will be updated to have an HP LeaveAccrualCode of 'Y' similar to exempt and classified employees
	- Proven? FALSE. However, you should not need to make any changes to account for this as this means there is no change how the type of timesheet is chosen for an employees. 
- Hourly leave will be included as time in calculating overtime. 
	- Proven? FALSE. This is NOT accounted for and you will need to update overtime calculations accordingly.
- Hourly leave time will be counted as both time AND leave.
	- Proven? ISH. The TIME file export job will create both time and leave entries in the respective generated files during export (and this is correct). However, there is ongoing discussion as to how best to mark time hours for leave for tax accounting purposes. The best solution here is still to be determined.

Also, note that this may be the last time Bellevue College provides any updates as our code base has moved away from the original collaborative distribution. You should consider and plan how your institution will maintain and update TLR for itself going forward.

## Setting up the project for development

#### Requirements ####

- .NET 3.5 framework
- Visual Studio 2012
	
Note: In the interest of keeping this version as similar framework-wise to the previous version to limit variables for folks, this version is still using .NET 3.5, though it can easily be upgraded to .NET 4.6 or 4.7. It is currently a Visual Studio 2012.

#### Clone project

Clone the project to a local repo. Ensure you are using the dev branch for development (or create a new one for your updates, depending on how extensive they are).

#### Update config files

Update the necessary .config files (`WebAppSettingXXX.config` and `WebConStrXXX.config` and update `Web.config` to point to the correct configSource file for your environment. 

#### Build project

Now build the TLR project. This should theoretically successfully build error free.Once built, you should be able to run it (with or without debugging) from Visual Studio. It is recommended, however, to set up your own local IIS server and set up the project application there.
