# TeacherSuite

TeacherSuite single page application will be a teacher's record book used in many different locations. For now TeacherSuite focuses on teaching programming courses. Teachers facilitating lessons will register absences, grades, prepare materials. There will be multiple locations with many groups, courses and teachers. TeacherSuite will allow for maintaining and proposing lesson materials.

![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)  ![Angular](https://img.shields.io/badge/angular-%23DD0031.svg?style=for-the-badge&logo=angular&logoColor=white) ![Postgres|105](https://img.shields.io/badge/postgres-%23316192.svg?style=for-the-badge&logo=postgresql&logoColor=white)  ![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white) ![JWT](https://img.shields.io/badge/JWT-black?style=for-the-badge&logo=JSON%20web%20tokens) 
### Setup
As of March 2026 - TeacherSuite is  available only on the development environment. It requires containarization software for running PostgreSQL and Keycloak.
### Technology stack
- Main stack: **Angular 21 + .NET 10**
- Code architecture: **Clean Architecture**
- Database: **PostgreSQL**
- Identity Provider: **Keycloak**
- Techniques: CQRS, Mediator pattern
- Libraries
    - MediatR
    - AutoMapper (to be removed)
    - Ardalis Guard Clauses
    - FluentValidation
    - Bogus
    - xUnit
    - RxJS

### User types
There will be 3 types of users:
- Admin - administering every functionality, accesses,
- Supervisor - administering locations, teachers, groups
- Teacher - registering lessons, maintaining materials
### Functionalities
-  [x] Teachers - maintaining teacher data, skills and groups assignments.
-  [x] Courses -  covers what to teach and for which age group.
-  [x] Groups - groups of students with a course and a teacher assigned to them.  
-  [x] Languages - for now TeacherSuite focuses on teaching programming languages
-  [ ] Age Groups
-  [ ] Students
-  [ ] Locations
-  [ ] Lessons
-  [ ] Lessons Plan
-  [ ] Grades
-  [ ] Materials
-  [ ] Substitutes

### Middleware
-  [x] Authentication
-  [x] Authorization
-  [x] Global Exception Handling
-  [x] Logging
-  [ ] Caching
-  [ ] RateLimiting

### Teachers
![Teachers](documentation/images/teachers.png)
![Create Teacher](documentation/images/teachers-create.png)

### Courses
![Courses](documentation/images/courses.png)

### Groups
![Groups](documentation/images/groups.png)