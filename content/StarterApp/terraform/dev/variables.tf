terraform {
  required_version = ">= 1.6"

  # Terraform Cloud holds the state. Backend blocks cannot take variables, so the two
  # values below are the only ones you edit by hand rather than pass through tfvars.
  cloud {
    organization = "your-org"
    workspaces {
      name = "dev-starterapp"
    }
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

# ─────────────────────────────────────────────
# Identity
# ─────────────────────────────────────────────

variable "project" {
  description = "Short project slug. Used to name every resource, so keep it lower case."
  type        = string
  default     = "starterapp"
}

variable "environment" {
  description = "Environment slug used in resource names."
  type        = string
  default     = "dev"
}

variable "aspnetcore_environment" {
  description = "Value of ASPNETCORE_ENVIRONMENT passed to the container."
  type        = string
  default     = "Test"
}

variable "aws_region" {
  type    = string
  default = "us-east-1"
}

# ─────────────────────────────────────────────
# Account-specific inputs
#
# Deliberately empty: these identify one organisation's AWS account, and a template that
# shipped real IDs would have every generated project pointing at someone else's
# infrastructure. Supply them through terraform.tfvars or Terraform Cloud workspace
# variables — see terraform.tfvars.example.
# ─────────────────────────────────────────────

variable "vpc_id" {
  description = "VPC the Fargate service runs in."
  type        = string
  default     = ""
}

variable "ecr_registry" {
  description = "ECR registry host, e.g. 000000000000.dkr.ecr.us-east-1.amazonaws.com"
  type        = string
  default     = ""
}

variable "certificate_arn" {
  description = "ACM certificate covering the API and UI host names. Must be in us-east-1 for CloudFront."
  type        = string
  default     = ""
}

variable "dns_zone_id_api" {
  description = "Route 53 zone for the API record. Empty skips DNS."
  type        = string
  default     = ""
}

variable "dns_zone_id_ui" {
  description = "Route 53 zone for the UI record. Empty skips DNS."
  type        = string
  default     = ""
}

variable "ui_domain_name" {
  description = "Fully qualified UI host name, e.g. starterapp-test.example.com"
  type        = string
  default     = ""
}

variable "ui_aliases" {
  description = "CloudFront alternate domain names. Empty falls back to the default *.cloudfront.net certificate."
  type        = list(string)
  default     = []
}

variable "inbound_cidr_blocks" {
  description = "CIDRs allowed to reach the API load balancer. Narrow this to your organisation's ranges for anything non-public."
  type        = list(string)
  default     = ["0.0.0.0/0"]
}

# ─────────────────────────────────────────────
# Capacity
# ─────────────────────────────────────────────

variable "task_cpu" {
  type    = number
  default = 512
}

variable "task_memory" {
  type    = number
  default = 1024
}

variable "desired_task_count" {
  type    = number
  default = 1
}

variable "min_task_count" {
  type    = number
  default = 1
}

variable "max_task_count" {
  type    = number
  default = 2
}
