$(function () {
    $.connection.hub.url = 'http://localhost:1980/signalr';
    var hub = $.connection.hub,
        chat = $.connection.chat,
        server = chat.server,
        client = chat.client,
        loginName;
    hub.start()
        .done(function () {
            $('#login').click(function () {
                server.login($('#name').val())
                    .done(function (name) {
                        loginName = name;
                        $('#login-box, #logout-box, #rooms-box, #chats-box').toggle();
                        $('#logged').html(loginName);
                    });
            });
            $('#logout').click(function () {
                server.logout($('#name').val())
                    .done(function () {
                        $('#login-box, #logout-box, #rooms-box, #chats-box').toggle();
                        $('#rooms, #chats').empty();
                    });
            });
            $('#new').click(function () {
                server.createRoom($('#room').val());
            });
        });
    client.rooms = function (rs) {
        $.each(rs, function (i, r) {
            $('#rooms').append($('<li/>')
                .html(r)
                .click(function () {
                    server.joinRoom($(this).text());
                }));
        });
    };
    client.join = function (r) {
        var $message = $('<input type="text"/>');
        var $li = $('<li/>')
            .append($('<h4/>').html(r))
            .append($('<div/>')
                .addClass('input')
                .append($message)
                .append($('<button/>')
                    .attr('id', 'send')
                    .text('Send')
                    .click(function () {
                        server.send(r, $message.val(),
                            loginName);
                    })
                )
            )
            .append($('<div/>')
                .addClass('messages-box')
                .append($('<ul />')
                    .attr('data-room', r)));
        $('#chats').append($li);
    };
    client.message = function (room, message) {
        $('[data-room="' + room + '"]')
        .prepend($('<li/>')
        .append($('<span/>')
        .addClass('sender')
        .html(message.sender))
        .append($('<span/>').html(message.message))
        );
    };
});