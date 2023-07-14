use std::sync::mpsc::Receiver;
use tokio::runtime;
use twitch_irc::{
    login::StaticLoginCredentials as LoginCreds,
    message::ServerMessage::Privmsg,
    ClientConfig as Config, SecureTCPTransport as TCPTransport, TwitchIRCClient as IRC,
};
use std::ffi::CString;
use std::ffi::c_char;

pub fn fake_main(receiver: Receiver<String>, callback: extern "C" fn(*mut c_char) -> ()) {
    let r = runtime::Runtime::new().unwrap();

    r.block_on(async move {
        let cfg = Config::default();
        let (mut incoming, client) = IRC::<TCPTransport, LoginCreds>::new(cfg);

        let twitch_recv_handle = tokio::spawn(async move {

            while let Some(msg) = incoming.recv().await {

                let Privmsg(msg) = msg else {
                    continue;
                };

                callback(CString::new(msg.message_text.as_str()).unwrap().into_raw());

            }
        });

        while let Ok(ch) = receiver.recv() {
            match client.join(ch) {
                Ok(_) => {}
                Err(e) => eprintln!("Wrong format! E: {e}"),
            }
        }

        drop(client);
        twitch_recv_handle.await.unwrap();
    });
}
