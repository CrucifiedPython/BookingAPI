# Booking API

---

## Table of Contents

1. [Running the Application](#running-the-application)
2. [Testing the Application](#testing-the-application)
3. [Architecture](#architecture)
4. [Filtering Logic](#filtering-logic)

---

## Running the Application

1. **Clone the repository**:

```bash
git clone https://github.com/CrucifiedPython/BookingAPI.git
cd BookingAPI
```

2. **Restore dependencies**:

```bash
dotnet restore
```

3. **Run the application**:

```bash
dotnet run --project BookingAPI
```

The API will start on `https://localhost:5177`

4. **Swagger / OpenAPI UI**:
   Visit `https://localhost:5177/swagger` to explore the endpoints interactively.

---

## Testing the Application

The solution contains two test projects:

1. **Integration tests**

These tests check input date validation, home creation, and retrieval of available homes under various scenarios.
Access these in `BookingAPI.IntegrationTests`. 

How to run:

```bash
cd BookingAPI.IntegrationTests
dotnet test
```

2. Performance tests

This project includes a single performance test for the repository, ensuring it handles a large number of entities (100K) efficiently.
Access these in `BookingAPI.PerformanceTests`

How to run:

```bash
cd BookingAPI.PerformanceTests
dotnet test
```

---

## Architecture

This project follows a simplified **Clean/Onion Architecture** with clear separation between API, business logic, and data access.

1. **API Layer (`Controllers`)**

    * Handles HTTP requests, model validation, and mapping input models to domain entities.

2. **Service Layer (`Services`)**

    * Contains business logic, e.g., validating dates and interacting with repositories.

3. **Repository Layer (`Repositories`)**

    * In-memory storage of homes and their available dates.


---

## Filtering Logic

### Indexing Homes
1. When a new Home is added, its `AvailableSlots` (list of `DateOnly`) are processed.
2. For each date in `AvailableSlots`, the home’s `Id` is inserted into the dictionary `_homesByDate`:
   - Key: `DateOnly` — an availability date.
   - Value: `HashSet<long>` — IDs of homes available on that date.

### Querying Homes
1. The requested date range (from `startDate` to `endDate`) is expanded into a list of required dates.
2. Homes available on the **first date** are retrieved from `_homesByDate`.
3. The set is iteratively intersected with sets from subsequent dates.
4. The result contains only homes available on **all requested dates**.

### Complexity
- Dictionary lookup per date: [O(1)](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.item)
- Set intersection per date: [O(n)](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1.intersectwith#remarks),
  where *n* = number of homes available on that date.
- Overall complexity: O(k × n), where *k* = number of days in the query.  
 
