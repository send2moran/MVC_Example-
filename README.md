MVC_Example-
============
This created as a Test for a company in Tel-Aviv 
to prove Asp.net MVC,  CSS & HTML5 Skills.

The Task:
============

Create web application with 3 screens:

1. Login
2. Main
3. Admin

The Login Screen:

The user can enter user and password:

a. If the combination is wrong: there will be shown an appropriate message
b. If the combination is wrong 3 times in a row, the ip will be blocked for a minute, and
the user will be blocked to a minute (that means that the user would be blocked
from anywhere)
c. After 1 minute blocking, the user will receive captcha.
d. The user can be manually disabled by the admin( appropriate message should be
displayed to the user)

The Main Screen:

Simple Table of login history of the users ( list of login date )

The Admin Screen:

In the admin screen, there will be options to delete user, add user, manually lock user, and
mark/unmark user as administrator.

•

The admin screen could only be shown to users that are marked as admin. It will
appear as a tab in the main screen.

Data Files:

All the user should be saved in an xml file called users.xml
All the login history should be saved in an csv or xml file called logins.(csv,xml)

-
-

•

Please note to read/write the data in a thread safe manner ( choose appropriate
lock method – and please explain it )

Users.xml will contain:

User, password (one way encrypted), IsAdmin, IsManuallyLocked

Login(csv or xml) will contain:

User, Success Login Date
