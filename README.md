# PopulatePointArrayDynamicLinks

A FactoryTalk Optix Project with DesignTime NetLogic that fills in the dynamic links of a two-dimensional point array (`[row, column]`) from two one-dimensional arrays. Creating these links one cell at a time in Optix Studio is slow.

For each row `i`, the script creates:

| Destination element | Linked to    |
|---------------------|--------------|
| `Destination[i,0]`  | `XValues[i]` |
| `Destination[i,1]`  | `YValues[i]` |

## Requirements

- FactoryTalk Optix Studio (built and tested against **1.8.0.555**)
- A **DesignTime** NetLogic. It runs in Studio, not at runtime.

## Setup

1. Copy `PointArraryNetLogic.cs` into your project's `ProjectFiles/NetSolution` folder. Or create a new DesignTime NetLogic in Optix Studio and paste in the class contents.
2. In Optix Studio, select the NetLogic object and add three variables of type **NodeId**:
   - `XValues`: one-dimensional array that holds the X values
   - `YValues`: one-dimensional array that holds the Y values
   - `Destination`: two-dimensional array (`[rows, 2]` or more columns) that will receive the links
3. Point each variable at the matching array node (drag the node onto the variable's value field).

The finished setup should look like this:

```
PointArraryNetLogic (NetLogic)
├── XValues      NodeId -> X
├── YValues      NodeId -> Y
└── Destination  NodeId -> PointArray
```

## Usage

1. Save the project so the NetLogic compiles.
2. In Optix Studio, right-click the NetLogic object and run **PopulateDynamicLinks**.
3. Check the **Output** pane. When it works, you'll see a message like:
   ```
   Created 20 dynamic links on PointArray
   ```
4. Open the `Destination` array and confirm the links, e.g. `Destination[0,0]` → `XValues[0]`.

## Validation and errors

The script checks every input before it changes anything. If a check fails, it writes an error to the Output log and leaves all links as they were.

| Log message | Cause |
|-------------|-------|
| `<Name> does not point to a valid variable` | A NodePointer is empty or points to something that isn't a variable. |
| `XValues and YValues must point to one-dimensional arrays` | A source isn't a one-dimensional array. |
| `Destination must point to a two-dimensional array with at least 2 columns` | The destination has the wrong shape. |
| `Array length mismatch: XValues=…, YValues=…, Destination rows=…` | The X length, Y length, and destination row count aren't all equal. |

## Behavior notes

- **Existing links are replaced.** Before it creates new links, the script deletes **every** dynamic link on the `Destination` variable. That includes links on columns after the second one.
- Links use the default **Read** mode.
- You can run the script more than once; each run rebuilds the links from the current arrays.
- The script uses array dimensions as declared in the model (`ArrayDimensions`), so give the arrays fixed sizes.
