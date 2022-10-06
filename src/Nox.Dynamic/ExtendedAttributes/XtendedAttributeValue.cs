
namespace Nox.Dynamic.ExtendedAttributes
{
    internal class XtendedAttributeValue 
    {
        public int EntityId { get; set; }

        public Guid UId { get; set; }

        public int EntityAttributeId { get; set; }

        public DateTimeOffset AsAtDateTime { get; set; }

    }
}

/*

CREATE TABLE    [dbox].[Value_string] (
[EntityId]      VARCHAR (64)       NOT NULL,
[UId]           UNIQUEIDENTIFIER   NOT NULL,
[AttributeId]   VARCHAR (64)       NOT NULL,
[AtDateTime]    DATETIMEOFFSET (7) NOT NULL,
[Value]         NVARCHAR (MAX)     NULL,
CONSTRAINT [PK_Value_string] PRIMARY KEY CLUSTERED ([EntityId] ASC, [GUID] ASC, [AtDateTime] DESC)
);              

 */