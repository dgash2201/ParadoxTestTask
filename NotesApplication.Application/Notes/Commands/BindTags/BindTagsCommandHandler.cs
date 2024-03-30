﻿using MediatR;
using NotesApplication.Application.Common.Repository;
using NotesApplication.Application.Common.Response;
using NotesApplication.Application.Tags.Commands.Create;
using NotesApplication.Domain;

namespace NotesApplication.Application.Notes.Commands.BindTags
{
    public class BindTagsCommandHandler : IRequestHandler<BindTagsCommand, Response<Note>>
    {
        private readonly IRepository<Note> _repository;
        private readonly IMediator _mediator;

        public BindTagsCommandHandler(IRepository<Note> repository, IMediator mediator)
        {
            _repository = repository;
            _mediator = mediator;
        }

        public async Task<Response<Note>> Handle(BindTagsCommand request, CancellationToken cancellationToken)
        {
            var contains = await _repository.ContainsAsync(x => x.Id == request.NoteId);

            if (!contains)
            {
                return new Response<Note>()
                {
                    IsSuccess = false,
                    Errors = new List<string>() { "Такого напоминания не существует\n" },
                };
            }

            var reminder = await _repository.GetAsync(request.NoteId);

            foreach (var tagName in request.TagNames)
            {
                var createTagCommand = new CreateTagCommand()
                {
                    Name = tagName,
                };

                var responseTag = await _mediator.Send(createTagCommand);

                if (responseTag.IsSuccess)
                {
                    reminder.Tags.Add(responseTag.Value);
                }
            }

            await _repository.SaveChangesAsync();

            return new Response<Note>()
            {
                IsSuccess = true,
                Value = reminder
            };
        }
    }
}