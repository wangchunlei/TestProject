$(function () {
    $.connection.hub.url = 'http://localhost:1980/signalr';
    var startPoint = null,
        currentPoint = null,
        surfaceLayer = $("#surface")[0],
        surfaceCtx = surfaceLayer.getContext("2d"),
        tempLayer = $("#temp")[0],
        tempCtx = tempLayer.getContext("2d"),
        hub = $.connection.hub,
        whiteboard = $.connection.whiteboard;

    surfaceCtx.font = "16px Arial";
    surfaceCtx.fillStyle = "#0F0";
    tempCtx.fillStyle = "#AAA";

    whiteboard.client.draw = function(shapes) {
        var prev = surfaceCtx.fillStyle;
        $.each(shapes, function(i, s) {
            surfaceCtx.fillStyle = "#F00";
            surfaceCtx.fillRect(
                s.x, s.y, s.width, s.height);
            surfaceCtx.fillStyle = "#000";
            surfaceCtx.fillText(s.from, s.x, s.y);
        });
        surfaceCtx.fillStyle = prev;
    };
    $('#temp')
        .on('mousedown', function(e) {
            if (!whiteboard.state.name) return;
            startPoint = {
                X: e.offsetX,
                Y: e.offsetY
            };
        })
        .on('mousemove', function(e) {
            if (!whiteboard.state.name) return;
            if (startPoint) {
                if (currentPoint) {
                    tempCtx.clearRect(
                        startPoint.X,
                        startPoint.Y,
                        currentPoint.X - startPoint.X,
                        currentPoint.Y - startPoint.Y);
                }
                currentPoint = {
                    X: e.offsetX,
                    Y: e.offsetY
                };
                tempCtx.fillRect(
                    startPoint.X,
                    startPoint.Y,
                    currentPoint.X - startPoint.X,
                    currentPoint.Y - startPoint.Y);
            }
        })
        .on('mouseup', function() {
            if (!whiteboard.state.name) return;
            if (startPoint) {
                var width = currentPoint.X - startPoint.X,
                    height = currentPoint.Y - startPoint.Y;
                tempCtx.clearRect(
                    startPoint.X,
                    startPoint.Y,
                    width,
                    height);
                surfaceCtx.fillRect(
                    startPoint.X,
                    startPoint.Y,
                    width,
                    height);
                whiteboard.server.draw(
                    startPoint.X,
                    startPoint.Y,
                    width,
                    height);
                currentPoint = null;
                startPoint = null;
            }
        });
    hub.start().done(function() {
        $('#join').click(function() {
            var name = $('#name').val();
            whiteboard.server.join(name)
                .done(function() {
                    $('#name').hide();
                    $('#join').hide();
                    $('#logged').html(name);
                })
                .fail(function(e) {
                    console.log(e);
                    $('#logged').html(e.message);
                });
        });
    });
});