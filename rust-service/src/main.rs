use std::convert::Infallible;
use std::net::SocketAddr;
use std::result::Result;

use hyper::service::{make_service_fn, service_fn};
use hyper::{Body, Client, Method, Request, Response, Server, StatusCode};

async fn handle_request(req: Request<Body>) -> Result<Response<Body>, anyhow::Error> {
    println!("{} {}", req.method(), req.uri().path());
    match (req.method(), req.uri().path()) {
        (&Method::GET, "/health-check") => Ok(Response::new(Body::from("Ok"))),

        (&Method::GET, "/api/Language") => {
            let answer = "{name: \"Rust\", machineName: \"wasi\"}";

            Ok(Response::builder()
                .status(StatusCode::OK)
                .body(Body::from(answer))
                .unwrap())
        }

        _ => Ok(Response::builder()
            .status(StatusCode::NOT_FOUND)
            .body(Body::from("Not Found"))
            .unwrap()),
    }
}

#[tokio::main(flavor = "current_thread")]
async fn main() -> Result<(), Box<dyn std::error::Error + Send + Sync>> {
    let consul_registration = r#"{
    "Name": "rust-service",
    "Address": "HOSTNAME",
    "Port": 80,
    "Check": {
      "HTTP": "http://HOSTNAME:80/health-check",
      "Interval": "10s",
      "Timeout": "5s"
    }
  }"#;
    let consul_registration = consul_registration.replace("HOSTNAME", "rust-service");

    println!("Consul registration: {}", consul_registration);

    let request = hyper::Request::put("http://consul:8500/v1/agent/service/register")
        .body(Body::from(consul_registration))?;

    println!("Request: {:?}", request);

    let client = Client::new();

    let result = client.request(request).await?;

    println!("Registration: {:?}", result);
    let body_bytes = hyper::body::to_bytes(result.into_body()).await?;
    let body_string = String::from_utf8(body_bytes.to_vec()).unwrap();
    println!("Body: {:?}", body_string);

    let addr = SocketAddr::from(([0, 0, 0, 0], 80));
    println!("Listening on http://{}", addr);
    let make_svc = make_service_fn(|_| async move {
        Ok::<_, Infallible>(service_fn(move |req| handle_request(req)))
    });
    let server = Server::bind(&addr).serve(make_svc);
    if let Err(e) = server.await {
        eprintln!("server error: {}", e);
    }
    Ok(())
}
