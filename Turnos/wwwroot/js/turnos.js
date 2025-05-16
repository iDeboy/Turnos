(() => {

    document.addEventListener('keypress', async e => {
        const event = {
            Key: e.key,
            Code: e.code,
            Location: e.Location,
            Repeat: e.Repeat,
            CtrlKey: e.ctrlKey,
            ShiftKey: e.ShiftKey,
            AltKey: e.altKey,
            MetaKey: e.metaKey,
            Type: e.type,
        }

        await DotNet.invokeMethodAsync('Turnos.Client', 'OnKeyPress', event);
    });

    document.addEventListener('keydown', async e => {
        const event = {
            Key: e.key,
            Code: e.code,
            Location: e.Location,
            Repeat: e.Repeat,
            CtrlKey: e.ctrlKey,
            ShiftKey: e.ShiftKey,
            AltKey: e.altKey,
            MetaKey: e.metaKey,
            Type: e.type,
        }

        await DotNet.invokeMethodAsync('Turnos.Client', 'OnKeyDown', event);
    });

})();