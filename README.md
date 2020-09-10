# ProjectIssuesManager

An application for managing issues, bugs and roles for projects. Built using ASP.NET Core MVC and PostgreSQL. 

There are three different roles and responsibilites that a signed up user potentially can have.

* Admin - Can see a list of all users, change user claims for all projects, create new projects, assign users to all projects, delete and manage details of projects and issues.
* Project Manager - Can see a list of all users, change user claims for one project, create new project, assign users to created projects, delete and manage details of specific projects only.
* Developer - can see list of issues assigned to developer, can see projects it is part of (without the ability to change details), create a new issue/delete its own issue.
