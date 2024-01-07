using AutoMapper;
using FormatWith;
using Centurion.Accounts.App.Embeds.Data;
using Centurion.Accounts.Core.Embeds;
using Centurion.Accounts.Core.Embeds.Services;
using Centurion.Accounts.Core.Services;

namespace Centurion.Accounts.Infra.Embeds.Services;

public class EmbedRenderer : IEmbedRenderer
{
  private readonly IMapper _mapper;
  private readonly IJsonSerializer _jsonSerializer;

  public EmbedRenderer(IMapper mapper, IJsonSerializer jsonSerializer)
  {
    _mapper = mapper;
    _jsonSerializer = jsonSerializer;
  }

  public async ValueTask<string> RenderAsync(DiscordEmbedWebHookBinding binding, IDictionary<string, object> context,
    CancellationToken ct = default)
  {
    var template = binding.MessageTemplate;

    var rendered = template with
    {
      Content = Render(template.Content, context),
      Username = Render(template.Username, context),
      Embeds = RenderEmbeds(template.Embeds, context)
    };

    var data = _mapper.Map<EmbedMessageData>(rendered);
    return await _jsonSerializer.SerializeAsync(data, ct);
  }

  private List<EmbedItem> RenderEmbeds(IReadOnlyCollection<EmbedItem> templateEmbeds,
    IDictionary<string, object> context)
  {
    var renderedEmbeds = new List<EmbedItem>(templateEmbeds.Count);
    foreach (var item in templateEmbeds)
    {
      renderedEmbeds.Add(item with
      {
        Title = Render(item.Title, context),
        Footer = item.Footer with
        {
          Text = Render(item.Footer.Text, context)
        },
        Fields = RenderFields(item.Fields, context)
      });
    }

    return renderedEmbeds;
  }

  private List<EmbedField> RenderFields(IReadOnlyCollection<EmbedField> embedFields,
    IDictionary<string, object> context)
  {
    var renderedFields = new List<EmbedField>(embedFields.Count);
    foreach (var field in embedFields)
    {
      renderedFields.Add(field with
      {
        Value = Render(field.Value, context)
      });
    }

    return renderedFields;
  }

  private static string Render(string template, IDictionary<string, object> context) => template.FormatWith(context);
}