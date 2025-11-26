# Final Report
This report summarises the development workflow, version control practices, testing strategy, and challenges encountered during the creation of the Sensor Solutions for Assignment.

## Development Process
I approached the project using a step-by-step, iterative development style. The first task was setting up a clean Git repository and organising the solution structure. From there, I implemented the system in small increments: sensor configuration loading, data generation, logging, data smoothing, anomaly detection, and optional fault simulation.
After each new feature was added, I ran tests, reviewed behaviour, and refined the logic before moving forward. This incremental approach helped keep the code stable and made debugging easier. Each improvement or adjustment was committed to version control with clear commit messages documenting the purpose of the change.

## Git Usage
Git played a major role in keeping the project manageable. Every new feature or significant fix was created on a separate branch to avoid breaking the main branch. After completion and testing, branches were merged back into the main line.
I made frequent commits, documenting progress so that it was easy to trace how the solution evolved over time. Tags were added to mark important stages such as “initial build,” “feature complete,” and “final version.”
I also configured GitHub Actions to run unit tests automatically whenever code was pushed. This ensured that nothing was merged unless the test suite passed, helping maintain project stability.

## Testing Practices
Testing formed a large part of the development cycle. Using xUnit, I wrote tests to verify sensor setup, simulated temperature readings, validation rules, and anomaly detection logic. Special attention was given to edge cases—for example:
empty or invalid configuration files


## Challenges Faced
Several obstacles came up along the way.
One issue was testing behaviour that depended on randomised temperature generation. Designing tests that remained reliable while still checking realistic behaviour required careful handling of boundaries and ranges.
Another challenge was dealing with occasional IO or file-locking errors during automated tests in Visual Studio Code. These sometimes caused tests to fail unpredictably. After investigating, I adjusted the test setup and cleanup routines to reduce these conflicts.
A further consideration was the potential feature of dynamic runtime configuration updates (for example, changing thresholds while the system was running). Implementing this would have required significant infrastructure such as command parsing, display updates, file modification, and error handling. I decided not to include this, as it would push the project beyond the assignment’s intended scope.

## Conclusion
Completing this project reinforced the importance of structured development, frequent testing, and proper use of version control. It provided hands-on experience in building a C# application supported by unit tests and continuous integration. Despite a few challenges, the system meets the assignment requirements and reflects meaningful learning in software development practices.