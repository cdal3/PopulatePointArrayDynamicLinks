#region Using directives
using System;
using System.Linq;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.CoreBase;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
#endregion

public class PointArraryNetLogic : BaseNetLogic
{
    [ExportMethod]
    public void PopulateDynamicLinks()
    {
        var xValues = GetPointedVariable("XValues");
        var yValues = GetPointedVariable("YValues");
        var destination = GetPointedVariable("Destination");
        if (xValues == null || yValues == null || destination == null)
            return;

        var xDims = xValues.ArrayDimensions;
        var yDims = yValues.ArrayDimensions;
        var destDims = destination.ArrayDimensions;

        if (xDims == null || xDims.Length != 1 || yDims == null || yDims.Length != 1)
        {
            Log.Error(LogicObject.BrowseName, "XValues and YValues must point to one-dimensional arrays");
            return;
        }
        if (destDims == null || destDims.Length != 2 || destDims[1] < 2)
        {
            Log.Error(LogicObject.BrowseName, "Destination must point to a two-dimensional array with at least 2 columns");
            return;
        }
        if (xDims[0] != yDims[0] || xDims[0] != destDims[0])
        {
            Log.Error(LogicObject.BrowseName,
                $"Array length mismatch: XValues={xDims[0]}, YValues={yDims[0]}, Destination rows={destDims[0]}");
            return;
        }

        foreach (var existingLink in destination.Refs.GetVariables(FTOptix.CoreBase.ReferenceTypes.HasDynamicLink).ToList())
            existingLink.Delete();

        string xPath = DynamicLinkPath.MakePath(destination, xValues);
        string yPath = DynamicLinkPath.MakePath(destination, yValues);
        uint linkCount = 0;

        for (uint row = 0; row < destDims[0]; row++)
        {
            AddElementLink(destination, $"{xPath}[{row}]", row, 0, linkCount++);
            AddElementLink(destination, $"{yPath}[{row}]", row, 1, linkCount++);
        }

        Log.Info(LogicObject.BrowseName, $"Created {linkCount} dynamic links on {destination.BrowseName}");
    }

    private IUAVariable GetPointedVariable(string pointerName)
    {
        var pointer = LogicObject.GetVariable(pointerName);
        var variable = pointer == null ? null : InformationModel.GetVariable(pointer.Value);
        if (variable == null)
            Log.Error(LogicObject.BrowseName, $"{pointerName} does not point to a valid variable");
        return variable;
    }

    private static void AddElementLink(IUAVariable destination, string sourcePath, uint row, uint column, uint linkNumber)
    {
        string browseName = linkNumber == 0 ? "DynamicLink" : $"DynamicLink{linkNumber}";
        var link = InformationModel.MakeVariable<DynamicLink>(browseName, FTOptix.Core.DataTypes.NodePath);
        link.Value = sourcePath;
        link.ParentElementAccess = new ElementAccess(new uint[] { row, column });
        destination.Refs.AddReference(FTOptix.CoreBase.ReferenceTypes.HasDynamicLink, link);
    }
}
