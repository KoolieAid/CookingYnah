use std::ffi::{c_void, c_char};
use std::sync::mpsc;
use std::thread;
use std::thread::JoinHandle;
use std::ffi::CString;
mod twitch;

#[repr(C)]
struct Context {
    thread_handle: JoinHandle<()>,
    client: mpsc::Sender<String>,
}

#[no_mangle]
pub extern "C" fn init_runtime(callback: Option<extern "C" fn(*mut c_char) -> ()>) -> *mut c_void {
    if callback.is_none() {
        return std::ptr::null_mut();
    }

    let (sender, receiver) = mpsc::channel::<String>();

    let handle = thread::spawn(move || twitch::fake_main(receiver, callback.unwrap()));

    let ctx = Context {
        thread_handle: handle,
        client: sender,
    };

    let handle = Box::new(ctx);
    handle.into()
}

#[no_mangle]
pub extern "C-unwind" fn free_handle(handle: *mut c_void) {
    unsafe {
        let handle: *mut Context = handle.cast();

        let ctx = Box::from_raw(handle);

        drop(ctx.client);

        match ctx.thread_handle.join() {
            Ok(_) => {},
            Err(_) => eprintln!("Unexpected error in thread."),
        }
    }
}

#[no_mangle]
pub extern "C-unwind" fn join_channel(ctx: *mut c_void, s_ptr: *const u16, s_len: i32) {
    use std::slice;

    let name = unsafe { slice::from_raw_parts(s_ptr, s_len as usize) };
    let name = String::from_utf16(name).unwrap();
    let ctx: Box<Context> = ctx.into();

    ctx.client.send(name).unwrap();

    Box::into_raw(ctx);
}

#[no_mangle]
pub unsafe extern "C-unwind" fn free_string(string: *mut c_char) {
    let cstr = CString::from_raw(string);
    drop(cstr);
}

impl From<*mut c_void> for Box<Context> {
    fn from(value: *mut c_void) -> Self {
        unsafe {
            let ctx: *mut Context = value.cast();
            Box::from_raw(ctx)
        }
    }
}

impl From<Box<Context>> for *mut c_void {
    fn from(value: Box<Context>) -> Self {
        Box::into_raw(value).cast()
    }
}
