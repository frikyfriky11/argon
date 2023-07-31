using System.Runtime.CompilerServices;

// This is needed to let the API layer set the properties that are intentionally
// marked as internal. Properties marked as internal are skipped during the OpenAPI
// spec generation but need to be model bound manually in the controllers.
// This is all done to observe the rules of REST APIs.
[assembly: InternalsVisibleTo("Argon.WebApi")]
[assembly: InternalsVisibleTo("Argon.Application.Tests")]