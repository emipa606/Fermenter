# GitHub Copilot Instructions for Fermenter (Continued) Mod

## Mod Overview and Purpose

Fermenter (Continued) is an update to the original Fermenter mod, adding a large industrial and powered fermenting/processing machine to the RimWorld game. This mod enhances the brewing and fermentation capabilities by providing a larger capacity and temperature self-regulation for the fermenting process. It also includes medieval tech options, vinegar production, plant oil extraction, dairy product making, and biofuel production from organics.

## Key Features and Systems

- **Industrial Fermenter**: A large, powered machine with self-regulation for temperature, allowing for more space-efficient fermentation processes.
- **Fermentation Vat**: A medieval tech option that is less space-efficient and more susceptible to poor fermentation temperature.
- **Vinegar Production**: Allows the creation of vinegar, which can be used to pickle food and for other products through additional fermentation.
- **Plant Oil Extraction**: Enables the production of oils from plants using solvents, which can be used as cooking aids to reduce cooking times.
- **Dairy Product Making**: Adds processes for making dairy products.
- **Biofuel Production**: The fermenter can be used to break down organics into bio-fuel (chemfuel).
- **Recipe Customization**: Easy to add new recipes using the mod's own fermentation def.

## Coding Patterns and Conventions

- **Namespaces**: Use namespaces to organize code logically, for example, `Fermenter` and `CookOil`.
- **Class Structure**: Keep classes focused and encapsulated, following the Single Responsibility Principle.
- **Method Names**: Use clear and descriptive names for methods, such as `GetOilReduction`, `GetProductIcon`, and `IsResearched`.
- **Constants and Enums**: Use constants and enums for fixed values, like `FixedValues` and `OilDefNames`.
- **Error Handling**: Implement error handling to manage unexpected situations gracefully.
- **Documentation**: Provide comments and XML documentation for public types and members to improve code readability and maintainability.

## XML Integration

- **Def Files**: Use XML definition files to define new game elements such as `FermentDef`, `RecipeDef`, and `SeedDef`.
- **Patch Files**: Create patch files to modify existing game behavior, ensuring compatibility with other mods.
- **Def File Structure**: Follow the RimWorld XML structure and conventions, such as using the `Defs` element to define new elements.

## Harmony Patching

- **Harmony**: Utilize the Harmony library to patch existing game code for functionality enhancements.
- **Patch Class Structure**: Organize patches into classes, each responsible for a specific set of changes.
- **Patch Method Naming**: Name patch methods clearly, indicating their purpose, such as `Patch_Fermenter` or `Patch_VinegarProduction`.
- **Patch Context**: Ensure patches are contextually relevant and do not conflict with other mods.

## Suggestions for Copilot

- **Suggest XML Definitions**: Generate XML definition files for new game elements, such as `FermentDef` and `RecipeDef`.
- **Suggest Harmony Patches**: Provide examples of Harmony patches for existing game code, focusing on functionality enhancements.
- **Suggest Coding Patterns**: Recommend coding patterns and conventions, such as using constants and enums for fixed values.
- **Suggest Recipe Customization**: Generate examples of adding new recipes to the fermenter using the mod's own fermentation def.

By following these guidelines, you can effectively use GitHub Copilot to enhance your Fermenter (Continued) mod, ensuring compatibility, functionality, and maintainability.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Temporary files

- Do not create temporary files, build output, test artifacts, or Copilot session
  folders inside the mod root.
- Store temporary artifacts under `%USERPROFILE%\CopilotTemp\[modname]`.
- Direct build and test output there as well, without changing project settings.
- Clean up task-specific temporary artifacts when finished.

## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

