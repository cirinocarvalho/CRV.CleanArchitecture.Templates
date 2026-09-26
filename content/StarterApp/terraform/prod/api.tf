# PLACEHOLDER: point this at your own Fargate module (a private Terraform Cloud registry
# module, a git source, or a local path) or replace the block with raw aws_ecs_* resources.
# As shipped, 'your-org' does not resolve and 'terraform init' fails until you do.
module "api" {
  source  = "app.terraform.io/your-org/api/fargate"
  version = "0.1.15"

  project     = var.project
  environment = var.environment
  vpc_id      = var.vpc_id
  aws_region  = var.aws_region

  database_port       = 5432
  dns_zone_id         = var.dns_zone_id_api
  inbound_cidr_blocks = var.inbound_cidr_blocks

  # /health/ready runs the database probe; /health/live only proves the process is up, so
  # a task with a dead connection string would stay in service.
  health_check_path = "/health/ready"

  task_cpu           = var.task_cpu
  task_memory        = var.task_memory
  desired_task_count = var.desired_task_count
  min_task_count     = var.min_task_count
  max_task_count     = var.max_task_count

  certificate_arn       = var.certificate_arn
  docker_image_override = "${var.ecr_registry}/${var.project}:${var.environment}-latest"

  template_env = [
    {
      "name" : "ASPNETCORE_ENVIRONMENT",
      "value" : var.aspnetcore_environment
    }
  ]
}
