namespace Gdc.Modules.Notif.Application.Rules;

public static class RuleTriggerTypes
{
    public const string Chronological = "chronological";

    public const string State = "state";
}

public static class TriggerReferences
{
    public const string FechaComparendo = "fecha_comparendo";

    public const string FechaNotificacion = "fecha_notificacion";
}

public static class QueueStatuses
{
    public const string Pending = "pending";

    public const string Processing = "processing";

    public const string Sent = "sent";

    public const string Failed = "failed";
}

public static class DeliveryStatuses
{
    public const string Delivered = "Entregado";

    public const string Bounced = "Rebotado";

    public const string Pending = "Pendiente";
}
