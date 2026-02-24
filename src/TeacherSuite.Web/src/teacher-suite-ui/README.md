# TeacherSuiteUi

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.0.1.

## Development server

### Running the Full Stack

The Angular frontend needs to communicate with the .NET backend API. Follow these steps:

1. **Start the backend API** (from the repository root):
   ```bash
   cd src/TeacherSuite.Web
   dotnet run --launch-profile https
   ```
   The API will run on `https://localhost:7030`

2. **Start the frontend dev server** (from this directory):
   ```bash
   npm start
   ```
   The frontend will run on `http://localhost:4200/`

The frontend is configured with a proxy (`proxy.conf.json`) that automatically forwards API requests (e.g., `/Teachers`, `/Courses`, `/AgeGroups`) to the backend at `https://localhost:7030`.

### Running Frontend Only

To start just the frontend development server without the backend:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
